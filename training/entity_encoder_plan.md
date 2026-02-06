# Entity Encoder Implementation Plan

## Problem Statement

Current training has:
- Explained variance → 0 (value function can't predict returns)
- Policy collapses to spamming one action

Root cause: Flat observation vector destroys relational structure. The model can't learn "unit A threatens city B" because everything is flattened into a grid.

## Solution: Entity-Based Observation Encoding

Replace flat observation with structured entity encoding using self-attention.

---

## Phase 1: Entity Observation Encoder (DO FIRST)

### Why This First
The value function needs relational understanding to predict returns. Without it, no architecture improvements to action selection will help.

### Architecture

```
Observation Input:
  scalars: [currency, score, turn, num_cities, ...]     → (10,)
  units:   [{type, x, y, hp, moved, attacked}, ...]     → (N, 6)
  cities:  [{x, y, level, population, production}, ...] → (M, 5)

                          ↓

Entity Embedding:
  scalar_emb = MLP(scalars)                             → (1, d)
  unit_embs  = MLP(units) + entity_type_emb[UNIT]       → (N, d)
  city_embs  = MLP(cities) + entity_type_emb[CITY]      → (M, d)

                          ↓

Concatenate: [scalar_emb, unit_embs, city_embs]         → (1+N+M, d)

                          ↓

Self-Attention (no positional encoding):
  - Entities attend to each other
  - Learns "unit A near city B" relationships
  - Permutation invariant (order doesn't matter)

                          ↓

Output:
  entity_embeddings: (1+N+M, d)  # For later action grounding
  state_embedding:   (d,)        # Mean pool for policy/value heads
```

### Key Design Choices

1. **No positional encoding** - Entities are an unordered set
2. **Entity type embedding** - Distinguish units from cities from global
3. **Padding mask** - Handle variable number of units/cities
4. **Small model** - Start with d=64, 2 layers, 4 heads

### Files to Create/Modify

```
training/
├── models/
│   ├── __init__.py
│   └── entity_encoder.py      # NEW: EntityObservationEncoder
├── train_rllib.py             # MODIFY: Use custom RLModule
└── entity_encoder_plan.md     # This file
```

### Implementation Steps

1. **Create `training/models/entity_encoder.py`**
   - `EntityObservationEncoder` nn.Module
   - Handles variable-length units/cities with padding
   - Self-attention over all entities
   - Returns pooled state + entity embeddings

2. **Create custom RLModule for RLlib**
   - Subclass `DefaultPPOTorchRLModule` (new API stack)
   - Override encoder to use EntityObservationEncoder
   - Keep default policy/value heads initially

3. **Modify `FlattenedPolyterraEnv`**
   - Change observation format from flat vector to dict:
     ```python
     {
       "scalars": np.array([...]),      # (10,)
       "units": np.array([...]),        # (max_units, 6) padded
       "cities": np.array([...]),       # (max_cities, 5) padded
       "unit_mask": np.array([...]),    # (max_units,) bool
       "city_mask": np.array([...]),    # (max_cities,) bool
     }
     ```

4. **Update observation space in env**
   - Change from `Box` to `Dict` space

### Expected Outcome
- Explained variance should improve (value function can reason about entities)
- May see more diverse action selection

---

## Phase 2: Action Embeddings with Cross-Attention

**Only proceed if Phase 1 shows improvement.**

### Architecture

```
State embedding (from Phase 1):  (d,)
Valid actions: [{type, unit_id, target_x, ...}, ...]

                          ↓

Action Embedding:
  For each action:
    type_emb = embed(action_type)                    → (d_action,)
    pos_emb  = MLP([target_x, target_y])             → (d_action,)

    If action involves unit:
      unit_emb = lookup from entity_embeddings       → (d,)  # GROUNDING

    action_emb = MLP(concat(type_emb, pos_emb, unit_emb))

                          ↓

Cross-Attention:
  Q = state_embedding                                → (1, d)
  K, V = action_embeddings                           → (num_actions, d)

  scores = softmax(Q @ K^T / sqrt(d))                → (num_actions,)

                          ↓

Apply action mask, sample action
```

### Key Insight: Entity Grounding
A "move unit 5" action should reference unit 5's embedding from the observation encoder. This creates semantic grounding - the action knows what it's operating on.

---

## Phase 3: Refinements (If Needed)

### Option A: Self-Attention Among Actions
If actions have complex interdependencies, add SAINT-style self-attention:
```
actions → self-attention → cross-attention with state → scores
```

### Option B: Turn-Level Planning
If step-by-step is limiting, consider autoregressive turn planning:
```
state → [action1, action2, ..., END_TURN] as a sequence
```

This is a bigger architectural change - only consider if Phases 1-2 hit a ceiling.

---

## Implementation Notes

### RLlib New API Stack
RLlib 2.x uses RLModule instead of TorchModelV2. To use custom encoder:

```python
from ray.rllib.core.rl_module.rl_module import RLModule
from ray.rllib.algorithms.ppo.torch.default_ppo_torch_rl_module import DefaultPPOTorchRLModule

class EntityEncoderPPORLModule(DefaultPPOTorchRLModule):
    def setup(self):
        # Custom encoder setup
        self.encoder = EntityObservationEncoder(...)
        # Use default heads
        super().setup()
```

Or use old API stack with:
```python
config.api_stack(enable_rl_module_and_learner=False)
```

### Hyperparameters to Tune
- `d_model`: 64 → 128 (embedding dimension)
- `n_heads`: 4 (attention heads)
- `n_layers`: 2 → 3 (transformer depth)
- `max_units`: 20 (padding size)
- `max_cities`: 10 (padding size)

### Debugging Checkpoints
1. After Phase 1: Check explained variance improves
2. After Phase 1: Visualize attention weights (what entities attend to what?)
3. After Phase 2: Check action distribution diversity

---

## References

- [Relational Deep RL](https://arxiv.org/abs/1806.01830) - Self-attention over entities
- [SAINT](https://arxiv.org/abs/2505.12109) - Attention for multi-action policies
- [AlphaStar](https://www.nature.com/articles/s41586-019-1724-z) - Entity embeddings + pointer networks
- [RLlib RLModule docs](https://docs.ray.io/en/latest/rllib/rl-modules.html)
