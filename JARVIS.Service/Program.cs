using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace JARVIS.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            getVoices();

            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<JARVISWorker>();
                })
                .Build()
                .Run();
        }

        public static void getVoices()
        {
            var synth = new SpeechSynthesizer();
            Console.WriteLine("Installed voices:");
            foreach (var v in synth.GetInstalledVoices())
                Console.WriteLine($" • {v.VoiceInfo.Name}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }



    public class JARVISWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                Modules.Logger.Log("JARVIS service started.");
                Modules.JARVIS.Startup(); // Starts your main backend logic
                while (!stoppingToken.IsCancellationRequested)
                {
                    Thread.Sleep(10000); // Keeps the worker alive
                }
            });
        }
    }
}