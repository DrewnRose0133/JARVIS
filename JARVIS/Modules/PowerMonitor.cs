
using System;

namespace JARVIS.Modules
{
    public static class PowerMonitor
    {
        public static bool IsApplianceDone(string name)
        {
            Logger.Log($"Checking power state for {name}...");
            return true; // Simulated result
        }

        public static void CheckAllAppliances()
        {
            if (IsApplianceDone("washer"))
            {
                VoiceOutput.Speak("Laundry cycle is complete.");
            }
        }
    }
}
