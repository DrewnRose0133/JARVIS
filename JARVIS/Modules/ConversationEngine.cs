using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    public class ConversationEngine
    {
        private readonly ILogger<ConversationEngine> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _localAiClient;
        private readonly List<Dictionary<string, string>> _conversationHistory = new();

        private readonly string _modelName;

        public ConversationEngine(ILogger<ConversationEngine> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _modelName = _configuration["OpenAI:Model"] ?? "hermes";

            _localAiClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration["OpenAI:ApiUrl"] ?? "http://localhost:8080/v1/"),
                Timeout = TimeSpan.FromMinutes(5)
            };
            _localAiClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            Initialize();
        }

        public void Initialize()
        {
            _logger.LogInformation("🧠 Initializing ConversationEngine for model '{0}'…", _modelName);

            _conversationHistory.Clear();
            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "system",
                ["content"] = "You are JARVIS, a witty, helpful home automation AI with a dry sense of humor. Speak like a British assistant."
            });
        }

        public async Task<string> ProcessInputAsync(string input)
        {
            await WaitForLocalAIReadyAsync(_modelName);

            _logger.LogInformation("🎙️ User said: {Input}", input);

            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = input
            });

            var payload = new
            {
                model = _modelName,
                messages = _conversationHistory
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                _logger.LogInformation("📤 Sending chat request to LocalAI: {0}", json);

                response = await _localAiClient.PostAsync("chat/completions", content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ LocalAI returned error {0}: {1}", response.StatusCode, rawResponse);
                    return $"Sorry, LocalAI returned error {response.StatusCode}.";
                }

                var parsedResponse = JsonDocument.Parse(rawResponse);

                var extractedReply = parsedResponse.RootElement
                               .GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();

                _conversationHistory.Add(new Dictionary<string, string>
                {
                    ["role"] = "assistant",
                    ["content"] = extractedReply
                });

                Respond(extractedReply);
                return extractedReply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Exception while connecting to LocalAI.");
                return "Sorry, I couldn't connect to my AI core.";
            }


            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            var reply = doc.RootElement
                           .GetProperty("choices")[0]
                           .GetProperty("message")
                           .GetProperty("content")
                           .GetString();

            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "assistant",
                ["content"] = reply
            });

            Respond(reply);
            return reply;
        }

        public Task<string> ProcessTranscriptionAsync(string transcription)
        {
            return ProcessInputAsync(transcription);
        }

        private void Respond(string message)
        {
            _logger.LogInformation("🤖 JARVIS: {Message}", message);
            Console.WriteLine($"JARVIS: {message}");
            VoiceOutput.SpeakAsync(message).Wait();
        }

        private async Task WaitForLocalAIReadyAsync(string expectedModel, int timeoutSeconds = 120)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalSeconds < timeoutSeconds)
            {
                try
                {
                    var resp = await _localAiClient.GetAsync("models");
                    resp.EnsureSuccessStatusCode();

                    var content = await resp.Content.ReadAsStringAsync();
                    if (content.Contains(expectedModel))
                    {
                        _logger.LogInformation("✅ LocalAI is ready with model '{0}'", expectedModel);
                        return;
                    }

                    _logger.LogInformation("⏳ LocalAI is running, but model '{0}' not yet loaded…", expectedModel);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("⏳ Waiting for LocalAI: {0}", ex.Message);
                }

                await Task.Delay(3000);
            }

            throw new TimeoutException($"LocalAI did not become ready with model '{expectedModel}' after {timeoutSeconds} seconds.");
        }
    }
}
