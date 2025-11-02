using System.Collections.Generic;

namespace SoakOverflow;

/// <summary>
/// Interface for bot AI implementations.
/// </summary>
public interface IBot
{
    /// <summary>
    /// Decides actions for all agents in the current turn.
    /// </summary>
    List<AgentCommand> DecideActions(GameState gameState);
}

/// <summary>
/// Basic bot implementation that just hunkers down.
/// </summary>
public class BasicBot : IBot
{
    public List<AgentCommand> DecideActions(GameState gameState)
    {
        var commands = new List<AgentCommand>();

        foreach (var agent in gameState.MyAgents)
        {
            var command = new AgentCommand
            {
                AgentId = agent.AgentId,
                Combat = new HunkerDownAction()
            };
            commands.Add(command);
        }

        return commands;
    }
}
