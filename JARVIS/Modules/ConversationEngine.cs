using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace JARVIS.Modules
{
    public class ConversationEngine
    {
        private readonly ILogger<ConversationEngine> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _localAiClient;
        private readonly Dictionary<string, Timer> _reminders = new();
        private readonly List<Dictionary<string, string>> _conversationHistory = new();

        public ConversationEngine(
            ILogger<ConversationEngine> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // --- LocalAI HTTP client setup ---
            _localAiClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/v1/")
            };
            _localAiClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            Initialize();
        }

        public void Initialize()
        {
            _logger.LogInformation("Initializing ConversationEngine (LocalAI)…");

            _conversationHistory.Clear();
            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "system",
                ["content"] = "You are JARVIS, a helpful, sarcastic, intelligent AI with a British accent. You help with smart home tasks, reminders, and can hold witty conversations."
            });

            _logger.LogInformation("ConversationEngine is ready.");
        }

        /// <summary>
        /// Processes a single user utterance: commands first, then local-AI chat if no command matched.
        /// </summary>
        public async Task<string> ProcessInputAsync(string input)
        {
            _logger.LogInformation("Processing input: {Input}", input);

            // 1) Domain commands
            if (TryHandleCommand(input.ToLower()))
                return string.Empty;

            // 2) Add to history and call LocalAI
            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = input
            });

            var payload = new
            {
                model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo",
                messages = _conversationHistory
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to LocalAI: {Url}",
                _localAiClient.BaseAddress + "chat/completions");

            HttpResponseMessage response;
            try
            {
                response = await _localAiClient.PostAsync("chat/completions", content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to LocalAI.");
                return "Sorry, I couldn’t reach the local AI engine.";
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("LocalAI returned {StatusCode}", response.StatusCode);
                return $"Local AI error: {response.StatusCode}";
            }

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var reply = doc.RootElement
                           .GetProperty("choices")[0]
                           .GetProperty("message")
                           .GetProperty("content")
                           .GetString();

            // 3) Append assistant reply to history
            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "assistant",
                ["content"] = reply
            });

            Respond(reply);
            return reply;
        }

        // --- Convenience wrapper for hosting code to call ---
        public Task<string> ProcessTranscriptionAsync(string transcription)
            => ProcessInputAsync(transcription);

        // --- WakeWord-based console loop (optional) ---
        public void OnWakeWordDetected()
        {
            _logger.LogInformation("Wake word detected. Entering local console chat loop...");
            Console.WriteLine("JARVIS is listening. What can I do for you?");

            Initialize();

            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("JARVIS: Going idle.");
                    break;
                }
                ProcessInputAsync(input).Wait();
            }
        }

        // --- Domain command handling (lights, reminders) ---
        private bool TryHandleCommand(string input)
        {
            if (input.Contains("remind") || input.Contains("reminder"))
            {
                ParseAndSetReminder(input);
                return true;
            }
            if (input.Contains("turn on the light") || input.Contains("lights on"))
            {
                Respond("Lights have been turned on.");
                return true;
            }
            if (input.Contains("turn off the light") || input.Contains("lights off"))
            {
                Respond("Lights are now off.");
                return true;
            }
            return false;
        }

        private void ParseAndSetReminder(string input)
        {
            var task = "your task";
            var minutes = 1;
            foreach (var word in input.Split(' '))
            {
                if (int.TryParse(word, out var m))
                {
                    minutes = m;
                    break;
                }
            }
            SetReminder(task, minutes);
            Respond($"Setting reminder for '{task}' in {minutes} minutes.");
        }

        public void SetReminder(string task, int minutes)
        {
            var timer = new Timer(minutes * 60 * 1000);
            timer.Elapsed += async (s, e) =>
            {
                await VoiceOutput.SpeakAsync($"Reminder: {task}");
                timer.Stop();
            };
            timer.Start();
            _reminders[task] = timer;
            _logger.LogInformation("Reminder set: {Task} in {Minutes}m", task, minutes);
        }

        private void Respond(string message)
        {
            _logger.LogInformation("JARVIS: {Message}", message);
            Console.WriteLine($"JARVIS: {message}");
            VoiceOutput.SpeakAsync(message).Wait();
        }
    }
}
