
using System;
using System.Threading.Tasks;
using JARVIS.Modules.Devices.Interfaces;

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

    //Ring camera injector
    public class CameraController
    {
        private readonly ICameraService _cam;
        public CameraController(ICameraService cam) => _cam = cam;

        public async Task ShowDoorbellAsync()
        {
            var url = await _cam.GetLiveStreamUrlAsync("12345");
            await VoiceOutput.SpeakAsync("Streaming your front door now.");
            // e.g. push to Visualizer via WebSocketServer.Broadcast(...)
        }
    }
}
