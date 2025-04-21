
using System;

namespace JARVIS.Modules
{
    public static class SensorMonitor
    {
        public static bool IsWindowOpen(string location)
        {
            Logger.Log($"Checking if window is open in {location}...");
            return false; // Replace with sensor API later
        }

        public static bool IsDoorOpen(string location)
        {
            Logger.Log($"Checking if door is open in {location}...");
            return false;
        }
    }
}
