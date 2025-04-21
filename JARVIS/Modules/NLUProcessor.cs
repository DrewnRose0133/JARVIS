
using System;

namespace JARVIS.Modules
{
    public static class NLUProcessor
    {
        public static string ParseCommand(string input)
        {
            Logger.Log("NLU parsing: " + input);
            // Simulated parse
            if (input.Contains("turn on"))
                return "light on";
            return "unknown";
        }
    }
}
