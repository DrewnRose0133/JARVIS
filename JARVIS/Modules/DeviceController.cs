
using System;

namespace JARVIS.Modules
{
    public static class DeviceController
    {
        public static void ControlLight(string location, string state)
        {
            Logger.Log($"Light command: {location} -> {state}");
            SmartThings.ControlDevice($"{location} light", state);
        }

        public static void SetAC(string location, int temperature)
        {
            Logger.Log($"AC command: {location} -> {temperature} degrees");
            SmartThings.ControlDevice($"{location} thermostat", temperature.ToString());
        }
    }
}
