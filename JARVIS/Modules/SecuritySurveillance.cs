
using System;

namespace JARVIS.Modules
{
    public static class SecuritySurveillance
    {
        public static void AlertMotion(string location)
        {
            Logger.Log($"Motion detected at {location}");
            VoiceOutput.SpeakAsync($"Alert. Motion detected at {location}.");
        }

        public static void TriggerAlarm()
        {
            Logger.Log("Security alarm triggered!");
            VoiceOutput.SpeakAsync("Security breach. Activating alarm.");
        }

        public static void DisarmSystem(string user)
        {
            Logger.Log($"Security disarmed by {user}");
            VoiceOutput.SpeakAsync($"Security system disarmed by {user}");
        }
    }
}
