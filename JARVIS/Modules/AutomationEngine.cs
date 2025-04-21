
using System;
using System.Collections.Generic;

namespace JARVIS.Modules
{
    public static class AutomationEngine
    {
        public static void RunAutomationRules()
        {
            // Simulated rule engine
            Logger.Log("Checking automation rules...");
            string currentTime = DateTime.Now.ToString("HH:mm");
            if (currentTime == "20:00")
            {
                Logger.Log("Rule matched: 8 PM - Turn on porch light");
                DeviceController.ControlLight("porch", "on");
            }
        }
    }
}
