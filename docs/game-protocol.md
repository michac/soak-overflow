# Game Protocol

## Initialization Input

**First line:** one integer `myId`, for your player identification.

**Second line:** one integer `agentDataCount` for the number of agents on the grid.

**Next `agentDataCount` lines:** The following 6 inputs for each agent:

- `agentId`: unique id of this agent
- `player`: id of the player owning this agent
- `shootCooldown`: min number or turns between two shots for this agent
- `optimalRange`: the optimal shooting range of this agent
- `soakingPower`: the maximum wetness damage output of this agent
- `splashBombs`: the starting amount of splash bombs available to this agent

**Next line:** two integers `width` and `height` for the size of the grid.

**The next `width * height` lines:** The following 3 inputs for each tile on the grid:

- `x`: X coordinate (0 is leftmost)
- `y`: Y coordinate (0 is uppermost)
- `tile_type`:
  - `0` for an empty tile
  - `1` for a low cover
  - `2` for a high cover

## Input for one game turn

**First line:** one integer `agentCount` for the number of remaining agents on the grid.

**Next `agentCount` lines:** The following 6 inputs for each agent:

- `agentId`: unique id of this agent
- `x`: X coordinate (0 is leftmost)
- `y`: Y coordinate (0 is uppermost)
- `cooldown`: number of turns left until this agent can shoot again
- `splashBombs`: current amount of splash bombs available to this agent
- `wetness`: current wetness of the agent

**Next line:** one integer `myAgentCount` for the number of agents controlled by the player.

## Output

A single line per agent, preceded by its `agentId` and followed by its action(s):

**Up to one move action:**
- `MOVE x y`: Attempt to move towards the location x, y.

**Up to one combat action:**
- `SHOOT id`: Attempt to shoot agent agentId.
- `THROW x y`: Attempt to throw a splash bomb at the location x, y.
- `HUNKER_DOWN`: Hunker down to gain 25% damage reduction against enemy attacks this turn.

**Up to one message action:**
- `MESSAGE text`: Display text in the viewer. Useful for debugging.

Instructions are separated by semicolons. For example, consider the following line:

```
3;MOVE 12 3;SHOOT 5
```

This instructs agent 3 to move towards the coordinates (12, 3) and to shoot agent 5.

**Note:** The `agentId` at the start can be omitted. In that case, the actions are assigned to the agents in ascending order of `agentId`.
