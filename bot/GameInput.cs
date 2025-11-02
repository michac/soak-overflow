using System;
using System.Collections.Generic;

namespace SoakOverflow;

/// <summary>
/// Handles parsing of game input from stdin.
/// </summary>
public static class GameInput
{
    /// <summary>
    /// Reads initialization input and creates the initial game state.
    /// </summary>
    public static GameState ReadInitialization()
    {
        var myId = int.Parse(Console.ReadLine()!);
        var agentDataCount = int.Parse(Console.ReadLine()!);

        var agentDataById = new Dictionary<int, AgentData>();
        for (int i = 0; i < agentDataCount; i++)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            var agentData = new AgentData(
                AgentId: int.Parse(inputs[0]),
                Player: int.Parse(inputs[1]),
                ShootCooldown: int.Parse(inputs[2]),
                OptimalRange: int.Parse(inputs[3]),
                SoakingPower: int.Parse(inputs[4]),
                InitialSplashBombs: int.Parse(inputs[5])
            );
            agentDataById[agentData.AgentId] = agentData;
        }

        var dims = Console.ReadLine()!.Split(' ');
        var width = int.Parse(dims[0]);
        var height = int.Parse(dims[1]);

        var tilesByPosition = new Dictionary<Position, Tile>();
        for (int i = 0; i < height; i++)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            for (int j = 0; j < width; j++)
            {
                var x = int.Parse(inputs[3 * j]);
                var y = int.Parse(inputs[3 * j + 1]);
                var tileType = (TileType)int.Parse(inputs[3 * j + 2]);

                var position = new Position(x, y);
                tilesByPosition[position] = new Tile(position, tileType);
            }
        }

        return new GameState
        {
            MyId = myId,
            Width = width,
            Height = height,
            AgentDataById = agentDataById,
            TilesByPosition = tilesByPosition
        };
    }

    /// <summary>
    /// Reads turn input and updates the game state.
    /// </summary>
    public static void ReadTurnData(GameState gameState)
    {
        var agentCount = int.Parse(Console.ReadLine()!);

        var agentStatesById = new Dictionary<int, AgentState>();
        for (int i = 0; i < agentCount; i++)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            var agentState = new AgentState(
                AgentId: int.Parse(inputs[0]),
                Position: new Position(int.Parse(inputs[1]), int.Parse(inputs[2])),
                Cooldown: int.Parse(inputs[3]),
                SplashBombs: int.Parse(inputs[4]),
                Wetness: int.Parse(inputs[5])
            );
            agentStatesById[agentState.AgentId] = agentState;
        }

        var myAgentCount = int.Parse(Console.ReadLine()!);
        var myAgentIds = new List<int>();

        // The my agent IDs aren't explicitly provided, so we need to determine them
        // from the agent states based on which player they belong to
        foreach (var agentState in agentStatesById.Values)
        {
            var agentData = gameState.AgentDataById[agentState.AgentId];
            if (agentData.Player == gameState.MyId)
            {
                myAgentIds.Add(agentState.AgentId);
            }
        }

        gameState.AgentStatesById = agentStatesById;
        gameState.MyAgentIds = myAgentIds;
    }
}
