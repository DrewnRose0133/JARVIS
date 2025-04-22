using System;

namespace JARVIS.Modules
{
    public static class CommandRouter
    {
        public static void HandleCommand()
        {
            Console.Write("Command: ");
            string command = Console.ReadLine()?.ToLower();
            HandleCommand(command);
        }

        public static void HandleCommand(string command)
        {
            Logger.Log($"Command: {command}");

            switch (command)
            {
                case "hello":
                    PersonalityEngine.Speak("Hello. How may I assist you?");
                    break;
                case "open garage":
                    if (ApprovedUsers.IsApproved("Andrew"))
                        GarageController.OpenGarage();
                    else
                        PersonalityEngine.Speak("Access denied.");
                    break;
                case "start routine":
                    AutomationEngine.RunAutomationRules();
                    break;
                case "shut down":
                    PersonalityEngine.Speak("Shutting down.");
                    Environment.Exit(0);
                    break;
                default:
                    PersonalityEngine.Speak("I'm sorry, I didn't understand that.");
                    break;
            }
        }
    }
}