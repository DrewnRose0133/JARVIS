using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using KoenZomers.Ring.Api;
using KoenZomers.Ring.Api.Exceptions;
using JARVIS.Modules;
using JARVIS.Modules.Devices;
using JARVIS.Modules.Devices.Interfaces;
using static JARVIS.Modules.Devices.RingMotionService;
using static JARVIS.Modules.Devices.RingCameraService;
using System.Diagnostics;

namespace JARVIS.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var psi = new ProcessStartInfo("powershell",
                 "-NoProfile -ExecutionPolicy Bypass -File run-localai.ps1")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            Process.Start(psi);

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddEnvironmentVariables();
                })
                .ConfigureServices((ctx, services) =>
                {
                    var config = ctx.Configuration;

                    // Core JARVIS modules
                    services.AddSingleton<ConversationEngine>();
                    services.AddSingleton<PersonalityEngine>();
                    services.AddSingleton<AudioController>();
                    services.AddSingleton<WebSocketServer>();
                    services.AddSingleton<CommandRouter>();
                    services.AddSingleton<VoiceInput>();
                    services.AddSingleton<WakeWordListener>();

                    // MQTT client
                    services.AddSingleton<IMqttClient>(sp =>
                    {
                        var cfg = sp.GetRequiredService<IConfiguration>();
                        var factory = new MqttClientFactory();
                        var client = factory.CreateMqttClient();
                        var options = new MqttClientOptionsBuilder()
                            .WithTcpServer(
                                cfg["SmartHome:Mqtt:Broker"],
                                int.Parse(cfg["SmartHome:Mqtt:Port"] ?? "1883"))
                            .WithCredentials(
                                cfg["SmartHome:Mqtt:Username"],
                                cfg["SmartHome:Mqtt:Password"])
                            .WithCleanSession()
                            .Build();
                        client.ConnectAsync(options).GetAwaiter().GetResult();
                        return client;
                    });

                    // Ring integration toggle
                    var ringEnabled = config.GetValue<bool>("Ring:Enabled");
                    if (ringEnabled)
                    {
                        // Authenticate Ring Session
                        services.AddSingleton<Session>(sp =>
                        {
                            var cfg = sp.GetRequiredService<IConfiguration>();
                            var logger = sp.GetRequiredService<ILogger<Session>>();
                            var email = cfg["Ring:Email"];
                            var password = cfg["Ring:Password"];
                            var twoFactor = cfg["Ring:TwoFactorCode"];
                            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                                throw new InvalidOperationException("Ring:Email and Ring:Password must be set.");
                            var session = new Session(email, password);
                            try
                            {
                                session.Authenticate();
                                logger.LogInformation("Ring session authenticated without 2FA.");
                            }
                            catch (TwoFactorAuthenticationRequiredException)
                            {
                                if (string.IsNullOrWhiteSpace(twoFactor))
                                    throw new InvalidOperationException("TwoFactorCode is required for Ring 2FA.");
                                session.Authenticate(twoFactor);
                                logger.LogInformation("Ring session authenticated with 2FA.");
                            }
                            return session;
                        });

                        // HttpClient for Ring VOD
                        services.AddHttpClient("RingClient", c =>
                        {
                            c.BaseAddress = new Uri("https://api.ring.com/clients_api/");
                            c.DefaultRequestHeaders.Accept.Add(
                                new MediaTypeWithQualityHeaderValue("application/json"));
                        });

                        // Real services
                        services.AddSingleton<ICameraService, RingCameraService>();
                        services.AddSingleton<IRingMotionService, RingMotionService>();
                    }
                    else
                    {
                        // No-op services
                        services.AddSingleton<ICameraService, NoOpCameraService>();
                        services.AddSingleton<IRingMotionService, NoOpMotionService>();
                    }

                    // Other device services
                    services.AddSingleton<ILightsService, MqttLightsService>();
                    services.AddSingleton<IThermostatService, MqttThermostatService>();

                    // Hosted orchestrator
                    services.AddHostedService<JarvisHostedService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .UseConsoleLifetime()
                .UseWindowsService()
                .Build();

            await host.RunAsync();
        }
    }
}
