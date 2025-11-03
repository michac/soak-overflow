using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SoakOverflow;

/// <summary>
/// Handles parsing of game input from stdin.
/// </summary>
public static class GameInput
{
    // Compiled regex patterns for efficient parsing
    private static readonly Regex AgentDataPattern = new Regex(
        @"^(?<agentId>\d+) (?<player>\d+) (?<shootCooldown>\d+) (?<optimalRange>\d+) (?<soakingPower>\d+) (?<initialBombs>\d+)$",
        RegexOptions.Compiled);

    private static readonly Regex DimensionsPattern = new Regex(
        @"^(?<width>\d+) (?<height>\d+)$",
        RegexOptions.Compiled);

    private static readonly Regex AgentStatePattern = new Regex(
        @"^(?<agentId>\d+) (?<x>\d+) (?<y>\d+) (?<cooldown>\d+) (?<splashBombs>\d+) (?<wetness>\d+)$",
        RegexOptions.Compiled);

    private static readonly Regex TileTripletsPattern = new Regex(
        @"(?<x>\d+) (?<y>\d+) (?<tileType>\d+)",
        RegexOptions.Compiled);

    /// <summary>
    /// Reads initialization input and creates the initial game state.
    /// </summary>
    public static GameState ReadInitialization(TextReader input)
    {
        var myId = int.Parse(input.ReadLine()!);
        var agentDataCount = int.Parse(input.ReadLine()!);

        var agentDataById = new Dictionary<int, AgentData>();
        for (int i = 0; i < agentDataCount; i++)
        {
            var match = AgentDataPattern.Match(input.ReadLine()!);
            var agentData = new AgentData(
                AgentId: int.Parse(match.Groups["agentId"].Value),
                Player: int.Parse(match.Groups["player"].Value),
                ShootCooldown: int.Parse(match.Groups["shootCooldown"].Value),
                OptimalRange: int.Parse(match.Groups["optimalRange"].Value),
                SoakingPower: int.Parse(match.Groups["soakingPower"].Value),
                InitialSplashBombs: int.Parse(match.Groups["initialBombs"].Value)
            );
            agentDataById[agentData.AgentId] = agentData;
        }

        var dimsMatch = DimensionsPattern.Match(input.ReadLine()!);
        var width = int.Parse(dimsMatch.Groups["width"].Value);
        var height = int.Parse(dimsMatch.Groups["height"].Value);

        var tilesByPosition = new Dictionary<Position, Tile>();
        for (int i = 0; i < height; i++)
        {
            var line = input.ReadLine()!;
            var matches = TileTripletsPattern.Matches(line);
            foreach (Match match in matches)
            {
                var x = int.Parse(match.Groups["x"].Value);
                var y = int.Parse(match.Groups["y"].Value);
                var tileType = (TileType)int.Parse(match.Groups["tileType"].Value);

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
    public static void ReadTurnData(TextReader input, GameState gameState)
    {
        var agentCount = int.Parse(input.ReadLine()!);

        var agentStatesById = new Dictionary<int, AgentState>();
        for (int i = 0; i < agentCount; i++)
        {
            var match = AgentStatePattern.Match(input.ReadLine()!);
            var agentState = new AgentState(
                AgentId: int.Parse(match.Groups["agentId"].Value),
                Position: new Position(
                    int.Parse(match.Groups["x"].Value),
                    int.Parse(match.Groups["y"].Value)),
                Cooldown: int.Parse(match.Groups["cooldown"].Value),
                SplashBombs: int.Parse(match.Groups["splashBombs"].Value),
                Wetness: int.Parse(match.Groups["wetness"].Value)
            );
            agentStatesById[agentState.AgentId] = agentState;
        }

        var myAgentCount = int.Parse(input.ReadLine()!);
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
