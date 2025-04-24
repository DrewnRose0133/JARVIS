using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using KoenZomers.Ring.Api;
using JARVIS.Modules;
using JARVIS.Modules.Devices;
using JARVIS.Modules.Devices.Interfaces;

namespace JARVIS.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddEnvironmentVariables();
                })
                .ConfigureServices((ctx, services) =>
                {
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
                        var config = sp.GetRequiredService<IConfiguration>();
                        var factory = new MqttClientFactory();
                        var client = factory.CreateMqttClient();
                        var opts = new MqttClientOptionsBuilder()
                            .WithTcpServer(config["SmartHome:Mqtt:Broker"], int.Parse(config["SmartHome:Mqtt:Port"]))
                            .WithCredentials(config["SmartHome:Mqtt:Username"], config["SmartHome:Mqtt:Password"])
                            .WithCleanSession()
                            .Build();
                        client.ConnectAsync(opts).GetAwaiter().GetResult();
                        return client;
                    });

                    // Ring API Session
                    services.AddSingleton<Session>(sp =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();
                        var email = config["Ring:Email"];
                        var password = config["Ring:Password"];
                        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                            throw new InvalidOperationException(
                                "Ring credentials are not configured. Please set Ring:Email and Ring:Password.");
                        var session = new Session(email, password);
                        session.Authenticate();
                        return session;
                    });

                    // HttpClient for Ring VOD
                    services.AddHttpClient("RingClient", c =>
                    {
                        c.BaseAddress = new Uri("https://api.ring.com/clients_api/");
                    });

                    // Device services
                    services.AddSingleton<ILightsService, MqttLightsService>();
                    services.AddSingleton<IThermostatService, MqttThermostatService>();
                    services.AddSingleton<ICameraService, RingCameraService>();
                    services.AddSingleton<IRingMotionService, RingMotionService>();

                    // Hosted orchestrator
                    services.AddHostedService<JarvisHostedService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .UseWindowsService()
                .Build();

            await host.RunAsync();
        }
    }
}
