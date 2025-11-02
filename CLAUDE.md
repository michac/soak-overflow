# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a competitive programming bot for CodinGame's water-combat tactical game "Soak Overflow". The bot controls multiple agents on a grid-based battlefield, making decisions about movement, shooting, and using splash bombs.

## Build System

```bash
# Build the project
dotnet build

# Run the bot (for local testing if you have test input)
dotnet run --project bot
```

**Important:** The build process includes a custom MSBuild target (`MergeForSubmission`) that automatically runs after compilation. This executes `scripts/merge-for-submission.ps1` to generate a single-file submission at `output/submission.cs` for CodinGame.

### Manual Merge Script Execution

```bash
# From Git Bash or PowerShell
pwsh.exe scripts/merge-for-submission.ps1
```

The merge script:
- Combines all .cs files from bot/ directory
- Deduplicates using statements
- Removes namespace declarations (merges into single namespace)
- Outputs to `output/submission.cs`

## Architecture

### Core Components

**Models.cs** - All data structures using C# records:
- `Position` - 2D coordinates with Manhattan distance calculation
- `AgentData` - Static agent properties (cooldown, range, power, bombs)
- `AgentState` - Dynamic per-turn state (position, cooldown, remaining bombs, wetness)
- `GameState` - Complete game snapshot with agents, tiles, and convenience queries (`MyAgents`, `EnemyAgents`)
- Action types: `MoveAction`, `ShootAction`, `ThrowAction`, `HunkerDownAction`, `MessageAction`
- `AgentCommand` - Combines actions for a single agent (one move + one combat + optional message)

**GameInput.cs** - Input parsing:
- `ReadInitialization()` - One-time setup: player ID, agent data, grid map
- `ReadTurnData(GameState)` - Updates game state each turn with current agent positions/status

**Bot.cs** - AI strategy interface:
- `IBot` interface with single method: `DecideActions(GameState) -> List<AgentCommand>`
- `BasicBot` - Minimal implementation (all agents hunker down)
- **To create a new strategy:** Implement `IBot` and update `Program.cs` line 11

**Program.cs** - Game loop:
- `Player.Main()` entry point (required for CodinGame)
- Reads initialization once, then loops: read turn → decide actions → output commands

### Design Patterns

- **Strategy Pattern**: Swap AI implementations via `IBot` interface
- **Immutable Records**: All models use `record` for value semantics
- **File-Scoped Namespaces**: `namespace SoakOverflow;` at top of files

## Game Protocol

Full specification in `docs/game-protocol.md`. Key points:

**Agent Actions per Turn:**
- Up to one MOVE: `MOVE x y` - Move toward coordinates
- Up to one combat action:
  - `SHOOT id` - Attack agent by ID (requires cooldown ready)
  - `THROW x y` - Throw splash bomb at coordinates
  - `HUNKER_DOWN` - Gain 25% damage reduction this turn
- Optional: `MESSAGE text` - Debug output

**Output Format:**
```
<agentId>;<action1>;<action2>;<action3>
```

Example: `3;MOVE 12 3;SHOOT 5`

## Development Workflow

### Creating a New Bot Strategy

1. Add new class implementing `IBot` in `Bot.cs`:
```csharp
public class MySmartBot : IBot
{
    public List<AgentCommand> DecideActions(GameState gameState)
    {
        // Your AI logic here
        // Access gameState.MyAgents, gameState.EnemyAgents
        // Return AgentCommand for each agent
    }
}
```

2. Update `Program.cs` line 11:
```csharp
IBot bot = new MySmartBot();
```

3. Build (automatically generates submission):
```bash
dotnet build
```

4. Submit `output/submission.cs` to CodinGame

### Key GameState Queries

```csharp
// Your agents
foreach (var agent in gameState.MyAgents) { ... }

// Enemy agents
foreach (var enemy in gameState.EnemyAgents) { ... }

// Agent static data
var agentData = gameState.AgentDataById[agent.AgentId];
int range = agentData.OptimalRange;
int power = agentData.SoakingPower;

// Tiles
var tile = gameState.TilesByPosition[new Position(x, y)];
bool isCover = tile.Type != TileType.Empty;

// Distance calculation
int dist = agent.Position.ManhattanDistance(enemy.Position);
```

## Important Constraints

- **No external dependencies**: Only .NET Standard Library allowed
- **Single file submission**: CodinGame requires one .cs file (handled by merge script)
- **Traditional Main method**: Cannot use top-level statements (CodinGame compatibility)
- **Explicit usings**: File-scoped namespaces mean you must explicitly `using System;` etc. in merged file

## Common Issues

### Build Fails After Adding New File

If you add a new .cs file to bot/ and the build fails, ensure:
1. File has `namespace SoakOverflow;` declaration
2. File includes necessary `using` statements
3. File doesn't conflict with existing type names

### Merge Script Issues

The merge script expects:
- File-scoped namespaces (`namespace SoakOverflow;`)
- All files in same namespace
- No block-scoped namespaces with braces

### Submission Doesn't Work on CodinGame

Check `output/submission.cs`:
1. Has `class Player` with `static void Main(string[] args)`
2. All necessary `using` statements at top
3. No duplicate type definitions
4. All code within `namespace SoakOverflow;`
