
using System;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    public static class CommandRouter
    {
        public static void HandleCommand()
        {
            Console.Write("Command: ");
            string command = Console.ReadLine()?.ToLower();
            Logger.Log($"Received command: {command}");

            switch (command)
            {
                case "hello":
                    PersonalityEngine.Speak("Hello! How can I assist you?");
                    break;
                case "open garage":
                    if (ApprovedUsers.IsApproved("Andrew")) GarageController.OpenGarage();
                    else PersonalityEngine.Speak("Access denied.");
                    break;
                case "close garage":
                    if (ApprovedUsers.IsApproved("Andrew")) GarageController.CloseGarage();
                    else PersonalityEngine.Speak("Access denied.");
                    break;
                case "toggle garage":
                    if (ApprovedUsers.IsApproved("Andrew")) GarageController.ToggleGarage();
                    else PersonalityEngine.Speak("Access denied.");
                    break;
                case "personality sarcastic":
                    PersonalityEngine.SetPersonality("Sarcastic");
                    break;
                case "personality friendly":
                    PersonalityEngine.SetPersonality("Friendly");
                    break;
                case "personality professional":
                    PersonalityEngine.SetPersonality("Professional");
                    break;
                default:
                    PersonalityEngine.Speak("I'm sorry, I didn't understand that.");
                    break;
            }
        }
    }
}
