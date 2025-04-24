using System;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Routes and handles console commands by blocking on asynchronous speech calls.
    /// </summary>
    public class CommandRouter
    {
        private readonly PersonalityEngine _personalityEngine;
        private readonly ILogger<CommandRouter> _logger;

        public CommandRouter(
            PersonalityEngine personalityEngine,
            ILogger<CommandRouter> logger)
        {
            _personalityEngine = personalityEngine;
            _logger = logger;
        }

        /// <summary>
        /// Reads a command from the console and dispatches it.
        /// </summary>
        public void HandleCommand()
        {
            Console.Write("Command: ");
            string command = Console.ReadLine()?.ToLower();
            HandleCommand(command);
        }

        /// <summary>
        /// Processes the given command and invokes the appropriate action.
        /// </summary>
        /// <param name="command">User-entered command string.</param>
        public void HandleCommand(string command)
        {
            _logger.LogInformation("Command: {Command}", command);
            switch (command)
            {
                case "hello":
                    _personalityEngine.Speak("Hello. How may I assist you?")
                        .GetAwaiter().GetResult();
                    break;

                case "open garage":
                    if (ApprovedUsers.IsApproved("Andrew"))
                    {
                        GarageController.OpenGarage();
                    }
                    else
                    {
                        _personalityEngine.Speak("Access denied.")
                            .GetAwaiter().GetResult();
                    }
                    break;

                case "start routine":
                    AutomationEngine.RunAutomationRules();
                    break;

                case "shut down":
                    _personalityEngine.Speak("Shutting down.")
                        .GetAwaiter().GetResult();
                    Environment.Exit(0);
                    break;

                default:
                    _personalityEngine.Speak("I'm sorry, I didn't understand that.")
                        .GetAwaiter().GetResult();
                    break;
            }
        }
    }
}