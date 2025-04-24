using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JARVIS.Modules;

namespace JARVIS.Service
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // Register configuration options
                    services.Configure<OpenAIOptions>(context.Configuration.GetSection("OpenAI"));
                    services.Configure<AzureSpeechOptions>(context.Configuration.GetSection("AzureSpeech"));

                    services.AddHttpClient<OpenAIClient>();

                    // Register application services
                    services.AddSingleton<ConversationEngine>();
                    services.AddSingleton<OpenAIClient>();
                    services.AddSingleton<PersonalityEngine>();
                    services.AddSingleton<AudioController>();
                    services.AddSingleton<WebSocketServer>();
                    services.AddSingleton<CommandRouter>();
                    services.AddSingleton<JARVISService>();
                    services.AddSingleton<VoiceInput>();
                    services.AddSingleton<WakeWordListener>();


                    // Register hosted service for startup/shutdown orchestration
                    services.AddHostedService<JarvisHostedService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                })
                // Optionally enable running as Windows Service
                .UseWindowsService()
                .Build();

            await host.RunAsync();
        }
    }

}
