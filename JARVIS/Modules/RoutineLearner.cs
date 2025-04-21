
using System;
using System.Collections.Generic;

namespace JARVIS.Modules
{
    public static class RoutineLearner
    {
        private static List<string> logs = new List<string>();

        public static void LogAction(string action)
        {
            logs.Add($"{DateTime.Now}: {action}");
            Logger.Log($"Routine log: {action}");
        }

        public static IEnumerable<string> GetSuggestions()
        {
            return new List<string> { "You usually dim the lights at 9 PM. Want me to automate that?" };
        }
    }
}
