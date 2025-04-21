
using System;

namespace JARVIS.Modules
{
    public static class EnvAwareness
    {
        public static string GetTimeBasedMood()
        {
            var hour = DateTime.Now.Hour;
            if (hour < 6 || hour > 20) return "Calm";
            return "Energetic";
        }
    }
}
