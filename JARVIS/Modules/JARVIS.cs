namespace JARVIS.Modules
{
    public static class JARVIS
    {
        public static void Startup()
        {
            Logger.Log("Starting JARVIS AI system...");
            VoiceInput.StartListening();
            Scheduler.Start();
            AutomationEngine.RunAutomationRules();
            Diagnostics.RunChecks();
        }
    }
}