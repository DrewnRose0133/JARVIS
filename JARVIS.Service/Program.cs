using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using KoenZomers.Ring.Api;
using JARVIS.Modules;
using JARVIS.Modules.Devices;
using JARVIS.Modules.Devices.Interfaces;
using JARVIS.Service;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // MQTT client
        services.AddSingleton<IMqttClient>(sp =>
        {
            var factory = new MqttClientFactory();
            var client = factory.CreateMqttClient();
            var mqttConfig = configuration.GetSection("SmartHome:Mqtt");
            var broker = mqttConfig["Broker"];
            var port = int.Parse(mqttConfig["Port"] ?? "1883");
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithCredentials(mqttConfig["Username"], mqttConfig["Password"])
                .Build();
            try
            {
                client.ConnectAsync(options).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var logger = sp.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Failed to connect to MQTT broker {Broker}:{Port}", broker, port);
                throw;
            }
            return client;
        });

        // Ring session
        services.AddSingleton<Session>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var ringEmail = cfg["Ring:Email"];
            var ringPassword = cfg["Ring:Password"];
            if (string.IsNullOrWhiteSpace(ringEmail) || string.IsNullOrWhiteSpace(ringPassword))
            {
                throw new InvalidOperationException(
                    "Ring credentials are not configured. " +
                    "Please set Ring:Email and Ring:Password in appsettings.json or environment variables.");
            }
            var session = new Session(ringEmail, ringPassword);
            session.Authenticate();
            return session;
        });

        // HttpClient for Ring VOD
        services.AddHttpClient("RingClient", client =>
        {
            client.BaseAddress = new Uri("https://api.ring.com/clients_api/");
        });

        // Core JARVIS modules
        services.AddHttpClient<OpenAIClient>();
        services.AddSingleton<ConversationEngine>();
        services.AddSingleton<PersonalityEngine>();
        services.AddSingleton<AudioController>();
        services.AddSingleton<WebSocketServer>();
        services.AddSingleton<CommandRouter>();
        services.AddSingleton<VoiceInput>();
        services.AddSingleton<WakeWordListener>();

        // Device services
        services.AddSingleton<ILightsService, MqttLightsService>();
        services.AddSingleton<IThermostatService, MqttThermostatService>();
        services.AddSingleton<ICameraService, RingCameraService>();
        services.AddSingleton<IRingMotionService, RingMotionService>();

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
