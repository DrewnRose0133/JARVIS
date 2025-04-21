
using System;

namespace JARVIS.Modules
{
    public static class PushNotificationManager
    {
        public static void SendPush(string title, string message)
        {
            Logger.Log($"Push sent: {title} - {message}");
            // Extend this to integrate with PushBullet, Pushover, or a custom mobile app
        }
    }
}
