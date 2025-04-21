
using System.Collections.Generic;

namespace JARVIS.Modules
{
    public static class ContextMemory
    {
        private static List<string> memoryLog = new List<string>();

        public static void Remember(string fact)
        {
            memoryLog.Add(fact);
            Logger.Log("Context remembered: " + fact);
        }

        public static IEnumerable<string> Recall() => memoryLog;
    }
}
