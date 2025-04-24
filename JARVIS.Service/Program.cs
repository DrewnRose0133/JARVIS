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

namespace JARVIS.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
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
                    services.AddHttpClient<OpenAIClient>();
                   // services.AddSingleton<VoiceOutput>();
                    services.AddSingleton<ConversationEngine>();
                    services.AddSingleton<PersonalityEngine>();
                    services.AddSingleton<AudioController>();
                    services.AddSingleton<WebSocketServer>();
                    services.AddSingleton<CommandRouter>();
                    services.AddSingleton<VoiceInput>();
                    services.AddSingleton<WakeWordListener>();
                    services.AddSingleton<ICameraService, RingCameraService>();
                    services.AddSingleton<ILightsService, MqttLightsService>();
                    services.AddSingleton<IThermostatService, MqttThermostatService>();
                    services.AddSingleton<ICameraService, RingCameraService>();
                    services.AddSingleton<Modules.Devices.IRingMotionService, RingMotionService>();

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
