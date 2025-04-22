using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JARVIS.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<JARVISWorker>();
                })
                .Build()
                .Run();
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