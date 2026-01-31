# Polyterra RL Architecture Plan

## The Core Problem

We're training an agent to play Polytopia, a turn-based strategy game. This is hard because:

1. **Huge action space**: 512 possible actions per turn
2. **Variable action meanings**: Action index 5 might be "MOVE unit to (3,4)" one turn and "RESEARCH hunting" the next
3. **Sparse rewards**: Most actions give 0 immediate reward
4. **Long horizons**: Good moves pay off 10-20 turns later
5. **Opponent modeling**: Need to predict and counter enemy strategies

---

## Current Approach (Broken)

```
Observation (flat vector) → MLP → 512 logits → sample action index
```

**Why it fails:**

- MLP sees action index as just a number
- Can't learn "action 5 is good" because action 5 means different things
- Explained variance → 0 (value function can't predict anything)
- Policy collapses to spamming one action

---

## Solution 1: Self-Play with RLlib

### What is Self-Play?

Instead of training against a random/fixed opponent, both players use the **same policy network**. As the policy improves, so does the opponent.

```
┌─────────────────┐
│  Shared Policy  │
│    Network      │
└────────┬────────┘
         │
    ┌────┴────┐
    ▼         ▼
Player 0   Player 1
(our turn) (opponent)
```

### Why Self-Play Works

1. **Curriculum learning**: Opponent difficulty scales with agent ability
2. **Exploits get punished**: If agent finds a cheap trick, opponent learns to counter it
3. **Emergent strategies**: Complex behaviors emerge from competition
4. **Double the data**: Every game trains from both perspectives

### Self-Play Variants

#### 1. Naive Self-Play

Both players are always the latest policy.

```python
policy = latest_policy
player_0.act(policy)
player_1.act(policy)
```

**Problem**: Can lead to "forgetting" - new policy beats old policy but loses to even older ones.

#### 2. Fictitious Self-Play (FSP)

Train against a **mixture of past policies**.

```python
opponent_policy = random.choice(policy_history)
player_0.act(latest_policy)
player_1.act(opponent_policy)
```

**Benefit**: More robust, doesn't forget how to beat old strategies.

#### 3. Population-Based Training (PBT)

Maintain a **population of policies** that compete and evolve.

```python
population = [policy_1, policy_2, ..., policy_n]
# Periodically:
# - Evaluate all policies against each other
# - Clone good policies, mutate hyperparameters
# - Replace bad policies
```

**Benefit**: Diverse strategies, avoids local optima.

### RLlib Implementation

RLlib makes self-play easy:

```python
config = {
    "multiagent": {
        "policies": {
            "shared_policy": PolicySpec(),
        },
        "policy_mapping_fn": lambda agent_id, **kwargs: "shared_policy",
    },
}
```

Both agents map to the same policy. RLlib handles the rest.

---

## Solution 2: Attention over Actions

### The Insight

Instead of treating actions as indices, treat them as **objects with semantics**.

Each action has structure:

```python
action = {
    "type": "MOVE",      # What kind of action
    "unit_id": 5,        # Which unit
    "target": (3, 4),    # Where
    "cost": 0,           # Resource cost
}
```

The network should understand this structure, not just see "index 42".

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        OBSERVATION                          │
│  (game state: map, units, resources, etc.)                 │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │   State Encoder       │
              │   (CNN or MLP)        │
              └───────────┬───────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │   State Embedding     │
              │   (256-dim vector)    │
              └───────────┬───────────┘
                          │
         ┌────────────────┼────────────────┐
         │                │                │
         ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│  Action 0   │  │  Action 1   │  │  Action N   │
│  Embedding  │  │  Embedding  │  │  Embedding  │
│             │  │             │  │             │
│ type: MOVE  │  │ type: BUILD │  │ type: END   │
│ unit: 3     │  │ x: 5, y: 2  │  │             │
│ to: (4,5)   │  │ imp: FARM   │  │             │
└──────┬──────┘  └──────┬──────┘  └──────┬──────┘
       │                │                │
       └────────────────┼────────────────┘
                        │
                        ▼
              ┌───────────────────────┐
              │   Cross-Attention     │
              │                       │
              │   Q = state_emb       │
              │   K, V = action_embs  │
              └───────────┬───────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │   Action Scores       │
              │   (one per action)    │
              └───────────┬───────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │   Masked Softmax      │
              │   (invalid = -inf)    │
              └───────────┬───────────┘
                          │
                          ▼
                   Selected Action
```

### Action Embedding

Each action gets embedded based on its components:

```python
def embed_action(action):
    # Embed action type (learned embedding)
    type_emb = type_embedding[action["type"]]  # 32-dim

    # Embed target position (if applicable)
    if action["type"] in ["MOVE", "BUILD", "ATTACK"]: # there are more actions, such as harvest that are target dependent
        pos_emb = position_embedding(action["x"], action["y"])  # 32-dim
    else:
        pos_emb = zeros(32)

    # Embed unit (if applicable)
    if action["type"] in ["MOVE", "ATTACK"]:
        unit_emb = unit_embedding(action["unit_type"])  # 32-dim
    else:
        unit_emb = zeros(32)

    # Combine
    return MLP(concat(type_emb, pos_emb, unit_emb))  # 64-dim
```

### Cross-Attention Mechanism

```python
def compute_action_scores(state_emb, action_embs, mask):
    """
    state_emb: (batch, 256) - encoded game state
    action_embs: (batch, num_actions, 64) - encoded actions
    mask: (batch, num_actions) - 1 for valid, 0 for invalid
    """
    # Project state to query
    Q = linear(state_emb)  # (batch, 64)
    Q = Q.unsqueeze(1)     # (batch, 1, 64)

    # Actions are keys and values
    K = action_embs        # (batch, num_actions, 64)
    V = ones_like(K)       # We just want scores, not values

    # Attention scores
    scores = (Q @ K.transpose(-1, -2)) / sqrt(64)  # (batch, 1, num_actions)
    scores = scores.squeeze(1)                      # (batch, num_actions)

    # Mask invalid actions
    scores = scores.masked_fill(mask == 0, -1e9)

    return scores  # Feed to softmax for action probs
```

### Why This Works

1. **Semantic understanding**: Network learns "MOVE actions go to positions" not "index 42 is good"
2. **Generalization**: New unit types or positions work automatically
3. **Attention focus**: Network can focus on relevant actions for current state
4. **Variable action count**: Naturally handles 5 or 500 valid actions

### Comparison to AlphaStar

DeepMind's AlphaStar (StarCraft II) uses similar ideas:

- **Pointer networks** for selecting units
- **Autoregressive action selection** (pick type, then target, then unit)
- **Transformer architecture** for processing game state

Our version is simpler but captures the key insight: **actions have structure**.

---

## Solution 3: Hierarchical Action Space

### The Idea

Instead of flat 512 actions, decompose into hierarchy:

```
Level 1: Action Type (8 choices)
    ├── END_TURN
    ├── MOVE      → Level 2: Which unit? (N choices)
    │                  → Level 3: Where? (M choices)
    ├── ATTACK    → Level 2: Which unit?
    │                  → Level 3: Which target?
    ├── BUILD     → Level 2: What improvement?
    │                  → Level 3: Where?
    ├── TRAIN     → Level 2: Which city?
    │                  → Level 3: What unit type?
    ├── RESEARCH  → Level 2: Which tech?
    └── ...
```

### Implementation Options

#### Option A: Autoregressive (Sequential)

```python
# Step 1: Pick action type
type_probs = policy.action_type_head(state)
action_type = sample(type_probs)

# Step 2: Pick parameters based on type
if action_type == MOVE:
    unit_probs = policy.unit_head(state, action_type)
    unit = sample(unit_probs)

    target_probs = policy.target_head(state, action_type, unit)
    target = sample(target_probs)
```

**Pros**: Small action space at each step
**Cons**: Sequential = slow, credit assignment is harder

#### Option B: Parallel with Masking

```python
# All heads in parallel
type_logits = policy.type_head(state)      # (8,)
unit_logits = policy.unit_head(state)      # (max_units,)
target_logits = policy.target_head(state)  # (map_size^2,)

# Mask based on type
if sampled_type == MOVE:
    valid_units = get_movable_units()
    valid_targets = get_move_targets(unit)
```

**Pros**: Parallel = fast
**Cons**: More complex masking logic

---

## Comparison of Approaches

| Approach               | Complexity | Training Speed | Sample Efficiency | Generalization |
| ---------------------- | ---------- | -------------- | ----------------- | -------------- |
| Flat 512 (current)     | Low        | Fast           | Poor              | None           |
| Self-play only         | Low        | Medium         | Medium            | None           |
| Attention over actions | High       | Slow           | Good              | Excellent      |
| Hierarchical           | Medium     | Medium         | Medium            | Good           |
| Self-play + Attention  | High       | Slow           | Best              | Excellent      |

---

## Recommended Implementation Order

### Phase 1: Self-Play with RLlib (This Week)

- Modify `train_rllib.py` for proper self-play
- Keep flat action space for now
- Test if better opponent helps learning

**Success criteria**: Explained variance > 0.3, reward trending up

### Phase 2: Attention over Actions (If Phase 1 Fails)

- Custom PyTorch policy with action embeddings
- Integrate with RLlib
- May need to simplify observation first

**Success criteria**: Agent learns diverse strategies, beats random

### Phase 3: Population-Based Training (Optimization)

- Multiple policies competing
- Hyperparameter evolution
- Diverse strategy emergence

**Success criteria**: Robust agent that doesn't exploit single strategy

---

## Technical Implementation Details

### RLlib Self-Play Setup

```python
from ray.rllib.algorithms.ppo import PPOConfig
from ray.rllib.policy.policy import PolicySpec

config = (
    PPOConfig()
    .environment("polyterra_env")
    .multi_agent(
        policies={
            "main": PolicySpec(),
        },
        policy_mapping_fn=lambda agent_id, *args, **kwargs: "main",
    )
    .training(
        lr=3e-4,
        gamma=0.99,
        lambda_=0.95,
        entropy_coeff=0.01,
        vf_loss_coeff=0.5,
        train_batch_size=4096,
        sgd_minibatch_size=256,
    )
    .resources(num_gpus=0)
    .rollouts(num_rollout_workers=4)
)
```

### Custom Attention Policy (PyTorch)

```python
import torch
import torch.nn as nn
from ray.rllib.models.torch.torch_modelv2 import TorchModelV2

class AttentionActionPolicy(TorchModelV2, nn.Module):
    def __init__(self, obs_space, action_space, num_outputs, model_config, name):
        TorchModelV2.__init__(self, obs_space, action_space, num_outputs, model_config, name)
        nn.Module.__init__(self)

        # State encoder
        self.state_encoder = nn.Sequential(
            nn.Linear(obs_space.shape[0], 256),
            nn.ReLU(),
            nn.Linear(256, 256),
            nn.ReLU(),
        )

        # Action type embedding
        self.action_type_emb = nn.Embedding(8, 32)

        # Position embedding (for targets)
        self.pos_emb = nn.Linear(2, 32)

        # Action combiner
        self.action_encoder = nn.Linear(64, 64)

        # Attention
        self.query_proj = nn.Linear(256, 64)

        # Value head
        self.value_head = nn.Linear(256, 1)

    def forward(self, input_dict, state, seq_lens):
        obs = input_dict["obs"]
        action_info = input_dict["obs"]["action_info"]  # Need to pass this

        # Encode state
        state_emb = self.state_encoder(obs["flat"])

        # Encode actions
        action_embs = self.encode_actions(action_info)

        # Attention scores
        Q = self.query_proj(state_emb).unsqueeze(1)
        scores = torch.bmm(Q, action_embs.transpose(1, 2)).squeeze(1)

        # Store for value function
        self._state_emb = state_emb

        return scores, state

    def value_function(self):
        return self.value_head(self._state_emb).squeeze(-1)
```

---

## Resources

- [AlphaStar Paper](https://www.nature.com/articles/s41586-019-1724-z) - DeepMind's StarCraft II agent
- [OpenAI Five](https://openai.com/research/openai-five) - Dota 2, similar challenges
- [RLlib Multi-Agent](https://docs.ray.io/en/latest/rllib/rllib-env.html#multi-agent-and-hierarchical)
- [PettingZoo + RLlib](https://docs.ray.io/en/latest/rllib/rllib-env.html#pettingzoo-multi-agent)

---

## Notes

- Start simple, add complexity only when needed
- Monitor explained variance - if near 0, architecture is wrong
- Diverse action distribution is a good sign (not spamming one action)
- Save replays to visually verify behavior
