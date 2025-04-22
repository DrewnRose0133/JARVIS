using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    public static class JARVIS
    {
        public static void Startup()
        {
            Logger.Log("Starting JARVIS core system...");

            VoiceInput.StartListening(); // mic input
            Scheduler.Start();           // reminders/automation
            AutomationEngine.RunAutomationRules();
            Diagnostics.RunChecks();
            // Any other startup routines
        }
    }
}