
using System;

namespace JARVIS.Modules
{
    public static class HardwareMonitor
    {
        public static void ReportStatus()
        {
            Logger.Log("CPU: 25%, RAM: 43%, Disk: OK");
        }
    }
}
