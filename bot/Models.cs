using System;
using System.Collections.Generic;
using System.Linq;

namespace SoakOverflow;

/// <summary>
/// Represents a 2D position on the grid.
/// </summary>
public record Position(int X, int Y)
{
    public int ManhattanDistance(Position other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
}

/// <summary>
/// Tile types on the game grid.
/// </summary>
public enum TileType
{
    Empty = 0,
    LowCover = 1,
    HighCover = 2
}

/// <summary>
/// Represents a tile on the game grid.
/// </summary>
public record Tile(Position Position, TileType Type);

/// <summary>
/// Static agent data that doesn't change during the game.
/// </summary>
public record AgentData(
    int AgentId,
    int Player,
    int ShootCooldown,
    int OptimalRange,
    int SoakingPower,
    int InitialSplashBombs);

/// <summary>
/// Current state of an agent during a turn.
/// </summary>
public record AgentState(
    int AgentId,
    Position Position,
    int Cooldown,
    int SplashBombs,
    int Wetness);

/// <summary>
/// Complete game state for the current turn.
/// </summary>
public class GameState
{
    public int MyId { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public Dictionary<int, AgentData> AgentDataById { get; init; } = [];
    public Dictionary<Position, Tile> TilesByPosition { get; init; } = [];
    public Dictionary<int, AgentState> AgentStatesById { get; set; } = [];
    public List<int> MyAgentIds { get; set; } = [];

    public IEnumerable<AgentState> MyAgents =>
        MyAgentIds.Select(id => AgentStatesById[id]);

    public IEnumerable<AgentState> EnemyAgents =>
        AgentStatesById.Values.Where(a => AgentDataById[a.AgentId].Player != MyId);
}

/// <summary>
/// Base class for agent actions.
/// </summary>
public abstract record Action
{
    public abstract string ToCommandString();
}

public record MoveAction(Position Target) : Action
{
    public override string ToCommandString() => $"MOVE {Target.X} {Target.Y}";
}

public record ShootAction(int TargetAgentId) : Action
{
    public override string ToCommandString() => $"SHOOT {TargetAgentId}";
}

public record ThrowAction(Position Target) : Action
{
    public override string ToCommandString() => $"THROW {Target.X} {Target.Y}";
}

public record HunkerDownAction : Action
{
    public override string ToCommandString() => "HUNKER_DOWN";
}

public record MessageAction(string Text) : Action
{
    public override string ToCommandString() => $"MESSAGE {Text}";
}

/// <summary>
/// Complete set of actions for an agent in a turn.
/// </summary>
public class AgentCommand
{
    public int AgentId { get; init; }
    public MoveAction? Move { get; set; }
    public Action? Combat { get; set; } // SHOOT, THROW, or HUNKER_DOWN
    public MessageAction? Message { get; set; }

    public string ToOutputString()
    {
        var actions = new List<string> { AgentId.ToString() };

        if (Move is not null)
            actions.Add(Move.ToCommandString());

        if (Combat is not null)
            actions.Add(Combat.ToCommandString());

        if (Message is not null)
            actions.Add(Message.ToCommandString());

        return string.Join(";", actions);
    }
}
