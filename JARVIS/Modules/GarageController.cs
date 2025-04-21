
using System;

namespace JARVIS.Modules
{
    public static class GarageController
    {
        public static void ToggleGarage()
        {
            Logger.Log("Toggling garage door.");
            VoiceOutput.Speak("Garage door is now toggled.");
        }

        public static void OpenGarage()
        {
            Logger.Log("Opening garage door.");
            VoiceOutput.Speak("Garage door is opening.");
        }

        public static void CloseGarage()
        {
            Logger.Log("Closing garage door.");
            VoiceOutput.Speak("Garage door is closing.");
        }
    }
}
