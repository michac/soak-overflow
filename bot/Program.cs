using System;
using SoakOverflow;

class Player
{
    static void Main(string[] args)
    {
        // Read initialization input
        var gameState = GameInput.ReadInitialization(Console.In);

        // Create bot instance
        IBot bot = new BasicBot();

        // Game loop
        while (true)
        {
            // Read turn data
            GameInput.ReadTurnData(Console.In, gameState);

            // Decide actions for all agents
            var commands = bot.DecideActions(gameState);

            // Output commands
            foreach (var command in commands)
            {
                Console.WriteLine(command.ToOutputString());
            }
        }
    }
}
