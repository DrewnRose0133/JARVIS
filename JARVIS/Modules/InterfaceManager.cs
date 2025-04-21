
using System;

namespace JARVIS.Modules
{
    public static class InterfaceManager
    {
        public static void ShowMessage(string message)
        {
            Logger.Log("UI Display: " + message);
            // Extend with visualizer/mobile broadcast
        }

        public static void PushToMobile(string notification)
        {
            Logger.Log("Mobile push: " + notification);
            // Extend to REST/mobile
        }

        public static void UpdateDashboard(string status)
        {
            Logger.Log("Dashboard status: " + status);
            // Hook into web/mobile UI
        }
    }
}
