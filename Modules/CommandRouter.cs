
using System;

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
                    Console.WriteLine("Hello! How can I help you today?");
                    break;
                case "scene":
                    SceneManager.LoadScene("Evening");
                    break;
                case "exit":
                    Logger.Log("System shutting down.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
    }
}
