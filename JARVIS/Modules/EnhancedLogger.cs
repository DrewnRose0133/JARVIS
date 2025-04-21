
using System;
using System.IO;

namespace JARVIS.Modules
{
    public static class EnhancedLogger
    {
        private static readonly string logPath = "Config/jarvis_log.txt";

        public static void LogEvent(string type, string source, string message)
        {
            string entry = $"[{DateTime.Now}] [{type}] [{source}] {message}";
            Console.WriteLine(entry);
            File.AppendAllText(logPath, entry + Environment.NewLine);
        }
    }
}
