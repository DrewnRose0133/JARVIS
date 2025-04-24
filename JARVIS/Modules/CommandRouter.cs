using System;
using Microsoft.Extensions.Logging;
using JARVIS.Modules.Devices.Interfaces;

namespace JARVIS.Modules
{
    /// <summary>
    /// Routes and handles console commands by blocking on asynchronous calls.
    /// </summary>
    public class CommandRouter
    {
        private readonly PersonalityEngine _personalityEngine;
        private readonly ILogger<CommandRouter> _logger;
        private readonly ILightsService _lights;
        private readonly IThermostatService _thermostat;
        private readonly ICameraService _camera;
        private readonly IRingMotionService _motion;

        public CommandRouter(
            PersonalityEngine personalityEngine,
            ILogger<CommandRouter> logger,
            ILightsService lights,
            IThermostatService thermostat,
            ICameraService camera,
            IRingMotionService motion)
        {
            _personalityEngine = personalityEngine;
            _logger = logger;
            _lights = lights;
            _thermostat = thermostat;
            _camera = camera;
            _motion = motion;
        }

        /// <summary>
        /// Prompts the user and processes the console command.
        /// </summary>
        public void HandleCommand()
        {
            Console.Write("Command: ");
            var command = Console.ReadLine()?.ToLower() ?? string.Empty;
            HandleCommand(command);
        }

        /// <summary>
        /// Processes the given command and invokes the appropriate action.
        /// </summary>
        /// <param name="command">User-entered command string.</param>
        public void HandleCommand(string command)
        {
            _logger.LogInformation("Command: {Command}", command);

            // Greeting
            if (command == "hello")
            {
                _personalityEngine.Speak("Hello. How may I assist you?")
                    .GetAwaiter().GetResult();
                return;
            }

            // Open garage
            if (command == "open garage")
            {
                if (ApprovedUsers.IsApproved("Andrew"))
                {
                    GarageController.OpenGarage();
                }
                else
                {
                    _personalityEngine.Speak("Access denied.")
                        .GetAwaiter().GetResult();
                }
                return;
            }

            // Start automation rules
            if (command == "start routine")
            {
                AutomationEngine.RunAutomationRules();
                return;
            }

            // Shutdown
            if (command == "shut down")
            {
                _personalityEngine.Speak("Shutting down.")
                    .GetAwaiter().GetResult();
                Environment.Exit(0);
                return;
            }

            // Lights control: turn on/off {lightId}
            if (command.StartsWith("turn on "))
            {
                var id = command.Substring("turn on ".Length);
                _lights.SetLightStateAsync(id, true).GetAwaiter().GetResult();
                _personalityEngine.Speak($"Turned on {id}.")
                    .GetAwaiter().GetResult();
                return;
            }
            if (command.StartsWith("turn off "))
            {
                var id = command.Substring("turn off ".Length);
                _lights.SetLightStateAsync(id, false).GetAwaiter().GetResult();
                _personalityEngine.Speak($"Turned off {id}.")
                    .GetAwaiter().GetResult();
                return;
            }

            // Thermostat: set thermostat <zone> to <temp>
            if (command.StartsWith("set thermostat "))
            {
                var parts = command.Split(' ');
                if (parts.Length >= 5 && parts[3] == "to" && double.TryParse(parts[4], out var temp))
                {
                    var zone = parts[2];
                    _thermostat.SetTemperatureAsync(zone, temp).GetAwaiter().GetResult();
                    _personalityEngine.Speak($"Set {zone} thermostat to {temp}°.")
                        .GetAwaiter().GetResult();
                }
                else
                {
                    _personalityEngine.Speak("Please specify a thermostat zone and temperature, e.g.: set thermostat living-room to 72.")
                        .GetAwaiter().GetResult();
                }
                return;
            }

            // Camera snapshot: snapshot {cameraId}
            if (command.StartsWith("snapshot "))
            {
                var camId = command.Substring("snapshot ".Length);
                var path = _camera.TakeSnapshotAsync(camId).GetAwaiter().GetResult();
                _personalityEngine.Speak($"Snapshot saved to {path}.")
                    .GetAwaiter().GetResult();
                return;
            }

            // Motion monitoring: subscribe/unsubscribe motion {cameraId}
            if (command.StartsWith("subscribe motion "))
            {
                var camId = command.Substring("subscribe motion ".Length);
                _motion.StartMonitoringAsync(camId).GetAwaiter().GetResult();
                _personalityEngine.Speak($"Started motion monitoring for {camId}.")
                    .GetAwaiter().GetResult();
                return;
            }
            if (command.StartsWith("unsubscribe motion "))
            {
                var camId = command.Substring("unsubscribe motion ".Length);
                _motion.StopMonitoringAsync(camId).GetAwaiter().GetResult();
                _personalityEngine.Speak($"Stopped motion monitoring for {camId}.")
                    .GetAwaiter().GetResult();
                return;
            }

            // Fallback
            _personalityEngine.Speak("I'm sorry, I didn't understand that.")
                .GetAwaiter().GetResult();
        }
    }
}
