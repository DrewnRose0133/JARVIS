
using System;

namespace JARVIS.Modules
{
    public static class WakeWordListener
    {
        public static void Listen()
        {
            Logger.Log("Listening for wake word...");
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine()?.ToLower();
                if (input == "jarvis")
                {
                    Logger.Log("Wake word detected.");
                    CommandRouter.HandleCommand();
                }
            }
        }
    }
}
