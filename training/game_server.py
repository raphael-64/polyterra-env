"""
Flask server for playable Polytopia game.
Wraps PolyterraEnv and exposes REST API for the web interface.
"""
import sys
sys.path.insert(0, '../polyterra-env-py')

import json
import numpy as np
from flask import Flask, jsonify, request
from flask.json.provider import DefaultJSONProvider
from flask_cors import CORS
from polyterra_env import PolyterraEnv


class NumpyJSONProvider(DefaultJSONProvider):
    """Custom JSON provider that handles numpy types."""
    def default(self, obj):
        if isinstance(obj, np.integer):
            return int(obj)
        if isinstance(obj, np.floating):
            return float(obj)
        if isinstance(obj, np.ndarray):
            return obj.tolist()
        return super().default(obj)


app = Flask(__name__)
app.json = NumpyJSONProvider(app)
CORS(app)

# Global game instance
env = None
current_agent = None


def serialize_observation(obs):
    """Convert observation to JSON-serializable format."""
    return {
        "turn": int(obs.get("turn", 0)),
        "currency": int(obs.get("currency", 0)),
        "score": int(obs.get("score", 0)),
        "num_cities": int(obs.get("num_cities", 0)),
        "num_units": int(len(obs.get("units", []))),
        "num_kills": int(obs.get("num_kills", 0)),
        "num_casualties": int(obs.get("num_casualties", 0)),
        "available_techs": get_raw_techs(),
        "units": [
            {
                "id": u.get("id", i),
                "type": str(u.get("type", "")),
                "x": int(u.get("x", 0)),
                "y": int(u.get("y", 0)),
                "health": float(u.get("health", 0)),
                "moved": bool(u.get("moved", False)),
                "attacked": bool(u.get("attacked", False)),
            }
            for i, u in enumerate(obs.get("units", []))
        ],
        "cities": [
            {
                "name": str(c.get("name", "")),
                "x": int(c.get("x", 0)),
                "y": int(c.get("y", 0)),
                "level": int(c.get("level", 0)),
                "population": int(c.get("population", 0)),
            }
            for c in obs.get("cities", [])
        ],
        "tiles": [
            {
                "x": int(t.get("x", 0)),
                "y": int(t.get("y", 0)),
                "terrain": int(t.get("terrain", 0)),
                "owner": int(t.get("owner", 0)),
                "explored": int(t.get("explored", 0)),
                "visible": int(t.get("visible", 0)),
                "has_unit": int(t.get("has_unit", 0)),
                "unit_type": int(t.get("unit_type", 0)),
                "unit_owner": int(t.get("unit_owner", 0)),
                "unit_health": float(t.get("unit_health", 0)),
                "improvement_type": int(t.get("improvement_type", 0)),
                "improvement_level": int(t.get("improvement_level", 0)),
                "city_population": int(t.get("city_population", 0)),
                "is_capital": int(t.get("is_capital", 0)),
                "resource": int(t.get("resource", 0)),
            }
            for t in obs.get("tiles", [])
        ],
    }


def get_valid_actions():
    """Get valid actions for current agent."""
    global env, current_agent
    if env is None or current_agent is None:
        return {}

    raw_obs = env._raw_observations.get(current_agent, {})
    return raw_obs.get("valid_actions", {})


def get_raw_techs():
    """Get tech names from raw observation."""
    global env, current_agent
    if env is None or current_agent is None:
        return []

    raw_obs = env._raw_observations.get(current_agent, {})
    return raw_obs.get("available_techs", [])


@app.route('/api/reset', methods=['POST'])
def reset_game():
    """Start a new game."""
    global env, current_agent

    data = request.json or {}
    seed = data.get('seed')

    if env is not None:
        env.close()

    env = PolyterraEnv(num_players=2)
    env.reset(seed=seed)
    current_agent = env.agent_selection

    obs = env.observe(current_agent)
    valid_actions = get_valid_actions()

    return jsonify({
        "success": True,
        "agent": current_agent,
        "observation": serialize_observation(obs),
        "valid_actions": valid_actions,
        "game_over": False,
    })


@app.route('/api/state', methods=['GET'])
def get_state():
    """Get current game state."""
    global env, current_agent

    if env is None:
        return jsonify({"error": "No game in progress. Call /api/reset first."}), 400

    obs = env.observe(current_agent)
    valid_actions = get_valid_actions()

    # Check if game is over
    game_over = all(env.terminations.get(a, False) for a in env.possible_agents)
    winner = None
    if game_over:
        # Determine winner by score
        scores = {}
        for agent in env.possible_agents:
            agent_obs = env.observe(agent)
            scores[agent] = agent_obs.get("score", 0)
        winner = max(scores, key=scores.get)

    return jsonify({
        "agent": current_agent,
        "observation": serialize_observation(obs),
        "valid_actions": valid_actions,
        "game_over": game_over,
        "winner": winner,
        "rewards": dict(env.rewards) if hasattr(env, 'rewards') else {},
    })


@app.route('/api/action', methods=['POST'])
def execute_action():
    """Execute an action."""
    global env, current_agent

    if env is None:
        return jsonify({"error": "No game in progress. Call /api/reset first."}), 400

    data = request.json
    if not data:
        return jsonify({"error": "No action data provided"}), 400

    # Parse action
    action_type = data.get('action_type', 0)
    x = data.get('x', 0)
    y = data.get('y', 0)
    unit_idx = data.get('unit_idx', 0)
    param1 = data.get('param1', 0)
    param2 = data.get('param2', 0)

    action = (action_type, x, y, unit_idx, param1, param2)

    # Execute action
    prev_agent = current_agent
    env.step(action)

    reward = env.rewards.get(prev_agent, 0)
    info = env.infos.get(prev_agent, {})

    # Update current agent
    current_agent = env.agent_selection

    # Check if game is over
    game_over = all(env.terminations.get(a, False) for a in env.possible_agents)
    winner = None
    if game_over:
        scores = {}
        for agent in env.possible_agents:
            agent_obs = env.observe(agent)
            scores[agent] = agent_obs.get("score", 0)
        winner = max(scores, key=scores.get)

    # Get new observation
    if current_agent and current_agent in env.agents:
        obs = env.observe(current_agent)
        valid_actions = get_valid_actions()
    else:
        obs = {}
        valid_actions = {}

    return jsonify({
        "success": not info.get("invalid_action", False),
        "reward": reward,
        "info": info,
        "agent": current_agent,
        "observation": serialize_observation(obs),
        "valid_actions": valid_actions,
        "game_over": game_over,
        "winner": winner,
    })


@app.route('/api/end_turn', methods=['POST'])
def end_turn():
    """Convenience endpoint for ending turn."""
    global env, current_agent

    if env is None:
        return jsonify({"error": "No game in progress"}), 400

    # End turn action
    action = (0, 0, 0, 0, 0, 0)

    prev_agent = current_agent
    env.step(action)

    reward = env.rewards.get(prev_agent, 0)
    current_agent = env.agent_selection

    game_over = all(env.terminations.get(a, False) for a in env.possible_agents)
    winner = None
    if game_over:
        scores = {}
        for agent in env.possible_agents:
            agent_obs = env.observe(agent)
            scores[agent] = agent_obs.get("score", 0)
        winner = max(scores, key=scores.get)

    if current_agent and current_agent in env.agents:
        obs = env.observe(current_agent)
        valid_actions = get_valid_actions()
    else:
        obs = {}
        valid_actions = {}

    return jsonify({
        "success": True,
        "reward": reward,
        "agent": current_agent,
        "observation": serialize_observation(obs),
        "valid_actions": valid_actions,
        "game_over": game_over,
        "winner": winner,
    })


if __name__ == '__main__':
    print("Starting Polytopia Game Server...")
    print("Open polytopia_playable.html in your browser")
    print("API endpoints:")
    print("  POST /api/reset - Start new game")
    print("  GET  /api/state - Get current state")
    print("  POST /api/action - Execute action")
    print("  POST /api/end_turn - End current turn")
    app.run(host='0.0.0.0', port=5001, debug=True)
