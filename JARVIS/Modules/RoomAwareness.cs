
using System;

namespace JARVIS.Modules
{
    public static class RoomAwareness
    {
        public static string GetCurrentRoom()
        {
            Logger.Log("Determining current room...");
            return "Living Room";
        }
    }
}
