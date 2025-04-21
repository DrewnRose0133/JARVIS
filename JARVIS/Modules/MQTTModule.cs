
using System;

namespace JARVIS.Modules
{
    public static class MQTTModule
    {
        public static void Connect()
        {
            Logger.Log("Connecting to MQTT broker...");
        }

        public static void Publish(string topic, string payload)
        {
            Logger.Log($"MQTT publish: {topic} -> {payload}");
        }
    }
}
