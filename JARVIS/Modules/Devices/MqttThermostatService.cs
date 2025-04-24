using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JARVIS.Modules.Devices.Interfaces;

namespace JARVIS.Modules.Devices
{
 
        public class MqttThermostatService : IThermostatService, IAsyncDisposable
        {
            private readonly IMqttClient _client;
            private readonly ILogger<MqttThermostatService> _logger;

            public MqttThermostatService(
                IConfiguration configuration,
                ILogger<MqttThermostatService> logger)
            {
                _logger = logger;

                var broker = configuration["SmartHome:Mqtt:Broker"];
                var portText = configuration["SmartHome:Mqtt:Port"];
                int port = int.TryParse(portText, out var p) ? p : 1883;

                var mqttOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(broker, port)
                    .WithCredentials(
                        configuration["SmartHome:Mqtt:Username"],
                        configuration["SmartHome:Mqtt:Password"])
                    .WithCleanSession()
                    .Build();

                var factory = new MqttClientFactory();
                _client = factory.CreateMqttClient();
                _client.ConnectAsync(mqttOptions).GetAwaiter().GetResult();
                _logger.LogInformation("Connected to MQTT broker at {Broker}:{Port}", broker, port);
            }

            public async Task SetTemperatureAsync(string zoneId, double temperature)
            {
                var topic = $"home/thermostat/{zoneId}/set";
                var payload = temperature.ToString("F1");
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(Encoding.UTF8.GetBytes(payload))
                    .WithRetainFlag(false)
                    .Build();

                await _client.PublishAsync(message);
                _logger.LogInformation("Published temperature '{Temp}' to '{Topic}'", payload, topic);
            }

            public async Task<double?> GetCurrentTemperatureAsync(string zoneId)
            {
                var stateTopic = $"home/thermostat/{zoneId}/state";
                var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

                Task Handler(MqttApplicationMessageReceivedEventArgs args)
                {
                    if (args.ApplicationMessage.Topic == stateTopic)
                    {
                        var seq = args.ApplicationMessage.Payload;
                        byte[] data = seq.IsEmpty ? Array.Empty<byte>() : seq.ToArray();
                        var msg = Encoding.UTF8.GetString(data);
                        tcs.TrySetResult(msg);
                    }
                    return Task.CompletedTask;
                }

                _client.ApplicationMessageReceivedAsync += Handler;
                await _client.SubscribeAsync(stateTopic);
                _logger.LogInformation("Subscribed to thermostat state '{Topic}'", stateTopic);

                var result = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));
                _client.ApplicationMessageReceivedAsync -= Handler;

                if (result == tcs.Task && double.TryParse(tcs.Task.Result, out var temp))
                {
                    return temp;
                }

                _logger.LogWarning("Timeout or invalid temperature for zone '{Zone}'", zoneId);
                return null;
            }

            public async ValueTask DisposeAsync()
            {
                try
                {
                    if (_client.IsConnected)
                    {
                        await _client.DisconnectAsync();
                        _logger.LogInformation("Disconnected MQTT client.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while disconnecting MQTT client.");
                }
                _client.Dispose();
            }

        public Task SetThermostatAsync(string thermostatId, int temp)
        {
            throw new NotImplementedException();
        }

        public Task RaiseThermostatAsync(string thermostatId, int degree)
        {
            throw new NotImplementedException();
        }

        public Task LowerThermostatAsync(string thermostatId, int degree)
        {
            throw new NotImplementedException();
        }

        public Task GetThermostatTempAsync(string thermostatId)
        {
            throw new NotImplementedException();
        }
    }
    }
