
using System;

namespace JARVIS.Modules
{
    public static class SmartThings
    {
        public static void ControlDevice(string device, string action)
        {
            Logger.Log($"Sending command to SmartThings: {device} -> {action}");
            Console.WriteLine($"[Stub] {device} turned {action}");
        }
    }
}
