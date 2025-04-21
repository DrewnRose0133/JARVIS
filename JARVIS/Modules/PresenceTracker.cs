
using System;

namespace JARVIS.Modules
{
    public static class PresenceTracker
    {
        public static bool IsUserHome(string user)
        {
            Logger.Log($"Checking presence for user: {user}...");
            return user == "Andrew"; // Simulated match
        }
    }
}
