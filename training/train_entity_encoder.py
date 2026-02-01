"""
RLlib Self-Play PPO training for Polyterra with Entity Encoder.

Implements attention-based action scoring as described in ARCHITECTURE_PLAN.md:
- Actions are embedded based on their semantic components (type, position, unit, etc.)
- State is encoded into a fixed-size embedding
- Cross-attention computes action scores from state query and action keys
- Invalid actions are masked before softmax
"""
import os

# Remove RAY_RUNTIME_ENV_HOOK if set (can cause issues with uv-managed environments)
if "RAY_RUNTIME_ENV_HOOK" in os.environ:
    del os.environ["RAY_RUNTIME_ENV_HOOK"]

import numpy as np
from datetime import datetime
from typing import Dict, List, Tuple

import torch
import torch.nn as nn
import torch.nn.functional as F

from polyterra_env import PolyterraEnv
from polyterra_env.game_data_mappings import (
    UNIT_NAME_TO_IDX, TECH_NAME_TO_IDX, IMPROVEMENT_NAME_TO_IDX,
    UNIT_IDX_TO_NAME, TECH_IDX_TO_NAME, IMPROVEMENT_IDX_TO_NAME,
)

import ray
from ray.rllib.algorithms.ppo import PPOConfig
from ray.rllib.env.multi_agent_env import MultiAgentEnv
from ray.tune.registry import register_env
from ray.rllib.algorithms.callbacks import DefaultCallbacks
from ray.rllib.models import ModelCatalog
from ray.rllib.models.torch.torch_modelv2 import TorchModelV2
from ray.rllib.utils.annotations import override
from ray.rllib.utils.typing import ModelConfigDict, TensorType

from gymnasium import spaces

# Optional: W&B integration
try:
    import wandb
    HAS_WANDB = True
except ImportError:
    HAS_WANDB = False
    print("wandb not available, logging to console only")


# ============================================================================
# Constants
# ============================================================================

MAX_ACTIONS = 512
NUM_ACTION_TYPES = 16  # end_turn, move, attack, build, train, research, etc.
NUM_UNITS = 40
NUM_TECHS = 38
NUM_IMPROVEMENTS = 47
MAP_SIZE = 16

# Action type indices (matching PolyterraEnv)
ACTION_END_TURN = 0
ACTION_MOVE = 1
ACTION_ATTACK = 2
ACTION_BUILD = 3
ACTION_TRAIN = 4
ACTION_RESEARCH = 5
ACTION_CAPTURE = 13
ACTION_HARVEST = 14
ACTION_CITY_REWARD = 15


# ============================================================================
# Entity Encoder Model
# ============================================================================

class EntityEncoderModel(TorchModelV2, nn.Module):
    """
    Attention-based action policy for Polyterra.

    Architecture:
    1. State Encoder: Processes game state into a 256-dim embedding
    2. Action Embedder: Embeds each action based on its semantic components
    3. Cross-Attention: Scores actions using state as query, actions as keys
    4. Masked Softmax: Applies action mask before sampling

    The key insight is that actions have structure (type, position, unit, etc.)
    and the network should understand this structure rather than treating
    action indices as arbitrary numbers.
    """

    def __init__(
        self,
        obs_space: spaces.Space,
        action_space: spaces.Space,
        num_outputs: int,
        model_config: ModelConfigDict,
        name: str,
        **kwargs,
    ):
        TorchModelV2.__init__(self, obs_space, action_space, num_outputs, model_config, name)
        nn.Module.__init__(self)

        # Hyperparameters
        self.state_dim = 256
        self.action_embed_dim = 64
        self.num_attention_heads = 4

        # obs_flat dimension - hardcode since RLlib flattens multi-agent spaces weirdly
        # 10 scalars + 16*16*4 tile features = 1034
        obs_dim = 1034
        print(f"[EntityEncoderModel] obs_space type: {type(obs_space)}, obs_dim = {obs_dim}")

        # ===== State Encoder =====
        # Processes flattened observation into state embedding
        self.state_encoder = nn.Sequential(
            nn.Linear(obs_dim, 512),
            nn.ReLU(),
            nn.Linear(512, 256),
            nn.ReLU(),
            nn.Linear(256, self.state_dim),
            nn.ReLU(),
        )

        # ===== Action Embeddings =====
        # Learned embeddings for action components
        self.action_type_embed = nn.Embedding(NUM_ACTION_TYPES, 32)
        self.unit_type_embed = nn.Embedding(NUM_UNITS + 1, 16)  # +1 for None
        self.tech_embed = nn.Embedding(NUM_TECHS + 1, 16)  # +1 for None
        self.improvement_embed = nn.Embedding(NUM_IMPROVEMENTS + 1, 16)  # +1 for None

        # Positional encoding for coordinates (learned)
        self.pos_x_embed = nn.Embedding(MAP_SIZE + 1, 8)  # +1 for None/-1
        self.pos_y_embed = nn.Embedding(MAP_SIZE + 1, 8)

        # Action combiner: combines all action components
        # 32 (type) + 16 (unit) + 16 (tech) + 16 (improve) + 16 (pos) = 96 -> 64
        self.action_combiner = nn.Sequential(
            nn.Linear(96, 64),
            nn.ReLU(),
            nn.Linear(64, self.action_embed_dim),
        )

        # ===== Cross-Attention =====
        # Query projection for state
        self.query_proj = nn.Linear(self.state_dim, self.action_embed_dim)
        # Key/Value projections for actions (optional, can use action embeddings directly)
        self.key_proj = nn.Linear(self.action_embed_dim, self.action_embed_dim)

        # ===== Value Head =====
        self.value_head = nn.Sequential(
            nn.Linear(self.state_dim, 128),
            nn.ReLU(),
            nn.Linear(128, 1),
        )

        # Store for value function
        self._state_embedding = None
        self._action_embeddings = None

    def _embed_action_features(self, action_features: torch.Tensor) -> torch.Tensor:
        """
        Embed action features tensor into action embeddings.

        Args:
            action_features: (batch, MAX_ACTIONS, 6) tensor of feature indices
                Features: [action_type, unit_type, tech_idx, improve_idx, pos_x, pos_y]

        Returns:
            (batch, MAX_ACTIONS, action_embed_dim) tensor of action embeddings
        """
        batch_size = action_features.shape[0]
        device = action_features.device

        # Extract individual features
        action_type_idx = action_features[:, :, 0].long()  # (batch, MAX_ACTIONS)
        unit_type_idx = action_features[:, :, 1].long()
        tech_idx = action_features[:, :, 2].long()
        improve_idx = action_features[:, :, 3].long()
        pos_x = action_features[:, :, 4].long()
        pos_y = action_features[:, :, 5].long()

        # Clamp indices to valid ranges
        action_type_idx = action_type_idx.clamp(0, NUM_ACTION_TYPES - 1)
        unit_type_idx = unit_type_idx.clamp(0, NUM_UNITS)
        tech_idx = tech_idx.clamp(0, NUM_TECHS)
        improve_idx = improve_idx.clamp(0, NUM_IMPROVEMENTS)
        pos_x = pos_x.clamp(0, MAP_SIZE)
        pos_y = pos_y.clamp(0, MAP_SIZE)

        # Embed each component
        type_emb = self.action_type_embed(action_type_idx)  # (batch, MAX_ACTIONS, 32)
        unit_emb = self.unit_type_embed(unit_type_idx)  # (batch, MAX_ACTIONS, 16)
        tech_emb = self.tech_embed(tech_idx)  # (batch, MAX_ACTIONS, 16)
        improve_emb = self.improvement_embed(improve_idx)  # (batch, MAX_ACTIONS, 16)
        pos_emb = torch.cat([
            self.pos_x_embed(pos_x),
            self.pos_y_embed(pos_y)
        ], dim=-1)  # (batch, MAX_ACTIONS, 16)

        # Combine all components
        combined = torch.cat([type_emb, unit_emb, tech_emb, improve_emb, pos_emb], dim=-1)
        # (batch, MAX_ACTIONS, 96)

        action_embs = self.action_combiner(combined)  # (batch, MAX_ACTIONS, 64)
        return action_embs

    @override(TorchModelV2)
    def forward(
        self,
        input_dict: Dict[str, TensorType],
        state: List[TensorType],
        seq_lens: TensorType,
    ) -> Tuple[TensorType, List[TensorType]]:
        """
        Forward pass computing action logits.

        The observation contains:
        - "obs_flat": Flattened game state (batch, obs_dim)
        - "action_features": Action feature indices (batch, MAX_ACTIONS, 6)
        - "action_mask": Valid action mask (batch, MAX_ACTIONS)
        """
        obs = input_dict["obs"]

        # Handle different observation formats
        if isinstance(obs, dict):
            obs_flat = obs.get("obs_flat", None)
            action_mask = obs.get("action_mask", None)
            action_features = obs.get("action_features", None)
        else:
            obs_flat = obs
            action_mask = None
            action_features = None

        # Ensure obs_flat is a tensor
        if not isinstance(obs_flat, torch.Tensor):
            obs_flat = torch.tensor(obs_flat, dtype=torch.float32, device=self.device)

        batch_size = obs_flat.shape[0]
        device = obs_flat.device

        # Encode state
        state_emb = self.state_encoder(obs_flat)  # (batch, state_dim)
        self._state_embedding = state_emb

        # Embed actions using features from observation
        if action_features is not None:
            if not isinstance(action_features, torch.Tensor):
                action_features = torch.tensor(action_features, dtype=torch.long, device=device)
            action_embs = self._embed_action_features(action_features)
        else:
            # Fallback: use learned embeddings for action indices
            action_indices = torch.arange(MAX_ACTIONS, device=device).unsqueeze(0).expand(batch_size, -1)
            action_type_indices = action_indices % NUM_ACTION_TYPES
            action_embs = self.action_type_embed(action_type_indices)
            action_embs = F.pad(action_embs, (0, self.action_embed_dim - 32))

        self._action_embeddings = action_embs

        # Cross-attention: state attends to actions
        Q = self.query_proj(state_emb).unsqueeze(1)  # (batch, 1, action_embed_dim)
        K = self.key_proj(action_embs)  # (batch, MAX_ACTIONS, action_embed_dim)

        # Attention scores (scaled dot product)
        scores = torch.bmm(Q, K.transpose(1, 2)).squeeze(1)  # (batch, MAX_ACTIONS)
        scores = scores / (self.action_embed_dim ** 0.5)

        # Apply action mask
        if action_mask is not None:
            if not isinstance(action_mask, torch.Tensor):
                action_mask = torch.tensor(action_mask, dtype=torch.float32, device=device)
            scores = scores.masked_fill(action_mask == 0, -1e9)

        return scores, state

    @override(TorchModelV2)
    def value_function(self) -> TensorType:
        """Compute value estimate from stored state embedding."""
        if self._state_embedding is None:
            raise RuntimeError("forward() must be called before value_function()")
        return self.value_head(self._state_embedding).squeeze(-1)

    @property
    def device(self):
        return next(self.parameters()).device


# ============================================================================
# Environment Wrapper
# ============================================================================

class EntityEncoderEnv(MultiAgentEnv):
    """
    RLlib MultiAgentEnv wrapper for Polyterra with entity encoder support.

    Key differences from FlattenedPolyterraEnv:
    - Observation includes action features (type, unit, tech, improve, position indices)
    - Observation includes action_mask
    - Model receives structured action information for attention via observation
    """

    # Number of features per action for embedding
    # [action_type, unit_type, tech_idx, improve_idx, pos_x, pos_y]
    ACTION_FEATURES = 6

    def __init__(self, config=None):
        super().__init__()
        config = config or {}
        self.num_players = config.get("num_players", 2)
        self.map_size = config.get("map_size", 16)
        self.max_steps = config.get("max_steps", 2000)

        # Create the underlying PettingZoo env
        self.env = PolyterraEnv(
            num_players=self.num_players,
            map_size=self.map_size,
        )

        self.steps = 0

        # Observation dimensionality: 10 scalars + map_size^2 * 4 tile features
        self._obs_dim = 10 + self.map_size * self.map_size * 4

        # Agent IDs
        self._agent_ids = set(f"player_{i}" for i in range(self.num_players))
        self.possible_agents = [f"player_{i}" for i in range(self.num_players)]
        self.agents = []

        # Define spaces
        # Observation includes:
        # - obs_flat: flattened game state
        # - action_mask: valid action mask
        # - action_features: (MAX_ACTIONS, ACTION_FEATURES) tensor of action info
        self._obs_space = spaces.Dict({
            "obs_flat": spaces.Box(-1.0, 1.0, shape=(self._obs_dim,), dtype=np.float32),
            "action_mask": spaces.Box(0, 1, shape=(MAX_ACTIONS,), dtype=np.float32),
            "action_features": spaces.Box(
                0, max(NUM_ACTION_TYPES, NUM_UNITS, NUM_TECHS, NUM_IMPROVEMENTS, MAP_SIZE + 1),
                shape=(MAX_ACTIONS, self.ACTION_FEATURES),
                dtype=np.int32
            ),
        })
        self._action_space = spaces.Discrete(MAX_ACTIONS)

    @property
    def observation_space(self):
        return spaces.Dict({
            agent: self._obs_space for agent in self.possible_agents
        })

    @property
    def action_space(self):
        return spaces.Dict({
            agent: self._action_space for agent in self.possible_agents
        })

    def get_observation_space(self, agent_id):
        return self._obs_space

    def get_action_space(self, agent_id):
        return self._action_space

    def _flatten_obs(self, obs_dict) -> np.ndarray:
        """Convert dict observation to flat numpy array."""
        if isinstance(obs_dict, np.ndarray):
            return obs_dict

        # Scalar features (normalized)
        flat = [
            obs_dict.get('currency', 0) / 1000.0,
            obs_dict.get('score', 0) / 10000.0,
            obs_dict.get('turn', 0) / 100.0,
            obs_dict.get('num_cities', 0) / 10.0,
            obs_dict.get('num_kills', 0) / 100.0,
            obs_dict.get('num_casualties', 0) / 100.0,
            obs_dict.get('current_player_idx', 0) / float(self.num_players),
            obs_dict.get('player_id', 0) / float(self.num_players + 1),
            len(obs_dict.get('units', [])) / 50.0,
            len(obs_dict.get('cities', [])) / 20.0,
        ]

        # Tile grid features
        tile_grid = np.zeros((self.map_size, self.map_size, 4), dtype=np.float32)
        for tile in obs_dict.get('tiles', []):
            x, y = tile.get('x', 0), tile.get('y', 0)
            if 0 <= x < self.map_size and 0 <= y < self.map_size:
                tile_grid[x, y] = [
                    tile.get('explored', 0),
                    tile.get('owner', 0) / float(self.num_players + 1),
                    tile.get('has_unit', 0),
                    tile.get('terrain', 0) / 7.0,
                ]

        return np.array(flat + list(tile_grid.flatten()), dtype=np.float32)

    def _extract_action_features(self, action_list: List[Dict]) -> np.ndarray:
        """
        Extract feature indices from action list for embedding.

        Returns array of shape (MAX_ACTIONS, ACTION_FEATURES) where features are:
        [action_type, unit_type, tech_idx, improve_idx, pos_x, pos_y]
        """
        features = np.zeros((MAX_ACTIONS, self.ACTION_FEATURES), dtype=np.int32)

        for i, action in enumerate(action_list[:MAX_ACTIONS]):
            action_type = action.get("action_type", -1)
            if action_type < 0:
                continue  # Invalid/padding action

            # Action type (clamped)
            features[i, 0] = min(action_type, NUM_ACTION_TYPES - 1)

            # Unit type
            unit_type = action.get("unit_type", None)
            if unit_type and isinstance(unit_type, str):
                features[i, 1] = UNIT_NAME_TO_IDX.get(unit_type.lower(), 0)

            # Tech
            tech_name = action.get("tech_name", None)
            if tech_name and isinstance(tech_name, str):
                features[i, 2] = TECH_NAME_TO_IDX.get(tech_name.lower(), 0)

            # Improvement
            improve_name = action.get("improvement_type", None)
            if improve_name and isinstance(improve_name, str):
                features[i, 3] = IMPROVEMENT_NAME_TO_IDX.get(improve_name.lower(), 0)

            # Position (try various field names)
            x = action.get("to_x", action.get("x", action.get("target_x", action.get("city_x", -1))))
            y = action.get("to_y", action.get("y", action.get("target_y", action.get("city_y", -1))))

            if x is None or x < 0:
                x = MAP_SIZE  # "no position" sentinel
            if y is None or y < 0:
                y = MAP_SIZE

            features[i, 4] = min(x, MAP_SIZE)
            features[i, 5] = min(y, MAP_SIZE)

        return features

    def _get_obs_for_agent(self, agent: str, raw_obs: Dict) -> Dict:
        """Create observation dict for an agent."""
        obs_flat = self._flatten_obs(raw_obs)

        # Get action mask
        mask = raw_obs.get('valid_actions_mask', np.ones(MAX_ACTIONS, dtype=np.float32))
        mask = np.array(mask, dtype=np.float32)[:MAX_ACTIONS]

        # Extract action features for embedding
        action_list = raw_obs.get('valid_actions_list', [])
        action_features = self._extract_action_features(action_list)

        return {
            "obs_flat": obs_flat,
            "action_mask": mask,
            "action_features": action_features,
        }

    def reset(self, *, seed=None, options=None):
        self.env.reset(seed=seed)
        self.steps = 0

        self.agents = list(self.env.agents)

        obs = {}
        infos = {}
        for agent in self.agents:
            raw_obs = self.env.observe(agent)
            obs[agent] = self._get_obs_for_agent(agent, raw_obs)
            infos[agent] = {"action_mask": obs[agent]["action_mask"]}

        return obs, infos

    def step(self, action_dict):
        self.steps += 1

        obs = {}
        rewards = {}
        terminateds = {"__all__": False}
        truncateds = {"__all__": False}
        infos = {}

        # Process actions
        for agent in list(self.env.agents):
            if agent not in action_dict:
                continue
            if self.env.agent_selection != agent:
                continue

            action = int(action_dict[agent])
            self.env.step(action)

        # Update agents
        self.agents = list(self.env.agents)

        # Get observations and rewards
        for agent in self.agents:
            raw_obs = self.env.observe(agent)
            obs[agent] = self._get_obs_for_agent(agent, raw_obs)
            infos[agent] = {"action_mask": obs[agent]["action_mask"]}
            rewards[agent] = self.env.rewards.get(agent, 0.0)
            terminateds[agent] = self.env.terminations.get(agent, False)
            truncateds[agent] = self.env.truncations.get(agent, False)

        # Check termination
        all_done = all(
            self.env.terminations.get(a, False) or self.env.truncations.get(a, False)
            for a in self.env.possible_agents
        )
        truncated_by_steps = self.steps >= self.max_steps

        terminateds["__all__"] = all_done
        truncateds["__all__"] = truncated_by_steps

        return obs, rewards, terminateds, truncateds, infos

    def close(self):
        self.env.close()

    def render(self):
        pass


def env_creator(config):
    """Create Polyterra environment with entity encoder support."""
    return EntityEncoderEnv(config)


class EntityEncoderCallback(DefaultCallbacks):
    """Callback for entity encoder training metrics."""
    pass


# ============================================================================
# Main Training Loop
# ============================================================================

def main():
    print("=" * 60)
    print("POLYTERRA ENTITY ENCODER TRAINING")
    print("=" * 60)

    # Initialize Ray
    ray.init(ignore_reinit_error=True)

    # Register custom model
    ModelCatalog.register_custom_model("entity_encoder", EntityEncoderModel)

    # Initialize W&B if available
    if HAS_WANDB:
        training_dir = os.path.dirname(os.path.abspath(__file__))
        wandb.init(
            project="polyterra-rl",
            name=f"entity_encoder_{datetime.now().strftime('%Y%m%d_%H%M%S')}",
            dir=training_dir,
            config={
                "algorithm": "PPO",
                "framework": "torch",
                "model": "entity_encoder",
                "num_players": 2,
                "map_size": 16,
                "max_steps": 500,
                "train_batch_size": 2048,
                "training_iterations": 500,
            }
        )
        print("W&B initialized")

    # Register environment
    register_env("polyterra_entity", env_creator)

    # Create test env to verify spaces
    test_env = env_creator({"num_players": 2})
    print(f"\nObservation space: {test_env.observation_space}")
    print(f"Action space: {test_env.action_space}")
    test_env.close()

    # Training config
    training_iterations = 500
    checkpoint_freq = 50

    # Configure PPO with entity encoder
    config = (
        PPOConfig()
        .api_stack(
            enable_rl_module_and_learner=False,
            enable_env_runner_and_connector_v2=False,
        )
        .environment(
            env="polyterra_entity",
            env_config={
                "num_players": 2,
                "map_size": 16,
                "max_steps": 500,
            },
        )
        .framework("torch")
        .env_runners(
            num_env_runners=0,  # Local mode
            rollout_fragment_length=256,
        )
        .training(
            train_batch_size=2048,
            minibatch_size=128,
            num_epochs=4,
            lr=3e-4,
            gamma=0.99,
            lambda_=0.95,
            entropy_coeff=0.01,
            vf_loss_coeff=0.5,
            clip_param=0.2,
            grad_clip=0.5,
            model={
                "custom_model": "entity_encoder",
                "custom_model_config": {},
            },
        )
        .multi_agent(
            policies={"shared_policy"},
            policy_mapping_fn=lambda agent_id, episode, **kw: "shared_policy",  # noqa
        )
        .resources(num_gpus=0)
        .callbacks(EntityEncoderCallback)
        .reporting(
            min_sample_timesteps_per_iteration=1000,
        )
    )

    # Build algorithm
    print("\nBuilding PPO algorithm with Entity Encoder...")
    algo = config.build()

    # Create checkpoint directory
    training_dir = os.path.dirname(os.path.abspath(__file__))
    checkpoint_dir = os.path.join(
        training_dir,
        f"checkpoints/entity_encoder_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    )
    os.makedirs(checkpoint_dir, exist_ok=True)

    # Training loop
    print(f"\nTraining for {training_iterations} iterations...")
    print("-" * 60)

    best_reward = float('-inf')

    for i in range(training_iterations):
        result = algo.train()

        # Extract metrics
        env_runners = result.get("env_runners", {})
        mean_reward = env_runners.get("episode_return_mean", 0)
        if mean_reward == 0 or mean_reward is None:
            mean_reward = env_runners.get("episode_reward_mean", 0)
        episodes = env_runners.get("num_episodes", 0)
        timesteps = result.get(
            "num_env_steps_sampled_lifetime",
            env_runners.get("num_env_steps_sampled_lifetime", 0)
        )

        # Learner metrics
        learners = result.get("learners", {})
        policy_stats = learners.get("shared_policy", {})
        policy_loss = policy_stats.get("policy_loss", 0)
        vf_loss = policy_stats.get("vf_loss", 0)
        entropy = policy_stats.get("entropy", 0)
        vf_explained_var = policy_stats.get("vf_explained_var", 0)
        total_loss = policy_stats.get("total_loss", 0)
        kl_loss = policy_stats.get("mean_kl_loss", 0)

        print(
            f"Iter {i+1:3d} | reward: {mean_reward:7.2f} | episodes: {int(episodes):3d} | "
            f"timesteps: {int(timesteps):6d} | expl_var: {vf_explained_var:.3f} | "
            f"entropy: {entropy:.3f}"
        )

        # Log to W&B
        if HAS_WANDB:
            wandb.log({
                "reward/mean": mean_reward,
                "reward/min": env_runners.get("episode_return_min", 0),
                "reward/max": env_runners.get("episode_return_max", 0),
                "episodes": episodes,
                "timesteps": timesteps,
                "train/policy_loss": policy_loss,
                "train/vf_loss": vf_loss,
                "train/total_loss": total_loss,
                "train/entropy": entropy,
                "train/kl_loss": kl_loss,
                "train/vf_explained_var": vf_explained_var,
                "iteration": i + 1,
            })

        # Save checkpoint periodically
        if (i + 1) % checkpoint_freq == 0:
            path = algo.save(checkpoint_dir)
            print(f"  -> Saved checkpoint: {path}")

        # Track best
        if mean_reward > best_reward:
            best_reward = mean_reward
            algo.save(f"{checkpoint_dir}/best")
            print(f"  -> New best! reward={best_reward:.2f}")

    # Final save
    final_path = algo.save(f"{checkpoint_dir}/final")
    print(f"\nFinal checkpoint: {final_path}")

    algo.stop()
    ray.shutdown()

    if HAS_WANDB:
        wandb.finish()

    print("\nDone!")


if __name__ == "__main__":
    main()
