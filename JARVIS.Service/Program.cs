using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JARVIS.Modules;
using JARVIS.Modules.Devices.Interfaces;
using JARVIS.Modules.Devices;
using MQTTnet;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

namespace JARVIS.Service
{

    public class Program
    {

        public static async Task Main(string[] args)
        {

            var ringEmail = builder.Configuration["Ring:Email"];
            var ringPassword = builder.Configuration["Ring:Password"];

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cfg =>
                {
                    cfg.SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // configuration binding
                    services.Configure<OpenAIOptions>(context.Configuration.GetSection("OpenAI"));
                   // Configuration for Azure Speech to be added later to save on cost
                    //services.Configure<AzureSpeechOptions>(context.Configuration.GetSection("AzureSpeech"));
                   // services.Configure<SmartHomeOptions>(context.Configuration.GetSection("SmartHome"));

                    // register core services (instance-based modules)


                    // OpenAI Client
                    services.AddHttpClient<OpenAIClient>();
                   // services.AddSingleton<VoiceOutput>();

                    // Coversation Engine
                    services.AddSingleton<ConversationEngine>();

                    // Personality Engine
                    services.AddSingleton<PersonalityEngine>();

                    //Audio Controller
                    services.AddSingleton<AudioController>();

                    // Web Socket Service
                    services.AddSingleton<WebSocketServer>();

                    // Command Router
                    services.AddSingleton<CommandRouter>();

                    // Voice Input
                    services.AddSingleton<VoiceInput>();

                    // Wake Word Listener
                    services.AddSingleton<WakeWordListener>();

                    // Light Service
                    services.AddSingleton<ILightsService, MqttLightsService>();
                   

                    // Ring Camera and Snapshots
                    services.AddSingleton<ICameraService, RingCameraService>();
                    services.AddSingleton<IRingMotionService, RingMotionService>();

                    // Thermostat via MQTT
                    services.AddSingleton<IThermostatService, MqttThermostatService>();

                    // MQTT client factory / connection
                    services.AddSingleton<IMqttClient>(sp => {
                        var factory = new MqttClientFactory();
                        var client = factory.CreateMqttClient();
                        // configure client options here or via IOptions<MqttSettings>
                        return client;
                    });

                    builder.Services.AddSingleton<KoenZomers.Ring.Api.Session>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<KoenZomers.Ring.Api.Session>>();

                        // Create and immediately log in synchronously:
                        var session = new KoenZomers.Ring.Api.Session(ringEmail, ringPassword, logger);
                        session.LoginAsync().GetAwaiter().GetResult();

                        return session;
                    });

                    // hosted orchestrator
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
