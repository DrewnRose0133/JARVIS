
using System;

namespace JARVIS.Modules
{
    public static class CrossDevice
    {
        public static void SyncCommand(string command)
        {
            Logger.Log($"Cross-device sync: {command}");
        }
    }
}
