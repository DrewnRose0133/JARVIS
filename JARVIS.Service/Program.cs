using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JARVIS.Modules;

namespace JARVIS.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // bind configuration sections to options
                    services.Configure<OpenAIOptions>(context.Configuration.GetSection("OpenAI"));
                    services.AddHostedService<JarvisHostedService>();

                    // services.Configure<AzureSpeechOptions>(context.Configuration.GetSection("AzureSpeech"));


                    // hosted service to orchestrate startup/shutdown
                    services.AddHostedService<JarvisHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseWindowsService()
                .Build();

            await host.RunAsync();
        }
    }
}
