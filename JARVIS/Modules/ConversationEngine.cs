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
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly Dictionary<string, Timer> _reminders = new();
        private readonly List<Dictionary<string, string>> _conversationHistory = new();

        public ConversationEngine(ILogger<ConversationEngine> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _apiKey = _configuration["OpenAI:ApiKey"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            Initialize();
        }

        public void Initialize()
        {
            _logger.LogInformation("Initializing ConversationEngine...");

            _conversationHistory.Clear();
            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "system",
                ["content"] = "You are JARVIS, a helpful, sarcastic, intelligent AI with a British accent. You help with smart home tasks, reminders, and can hold witty conversations."
            });

            _logger.LogInformation("ConversationEngine is ready.");
        }

        public async Task<string> ProcessInputAsync(string input)
        {
            _logger.LogInformation("Processing input: {Input}", input);

            if (TryHandleCommand(input.ToLower()))
                return string.Empty;

            _conversationHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = input
            });

            var requestBody = new
            {
                model = "gpt-4",
                messages = _conversationHistory
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

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

        public void OnWakeWordDetected()
        {
            _logger.LogInformation("Wake word detected. Activating JARVIS conversation loop...");
            Console.WriteLine("JARVIS is listening. What can I do for you?");

            Initialize();

            while (true)
            {
                Console.Write("You: ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("JARVIS: Going idle.");
                    break;
                }

                ProcessInputAsync(input).Wait();
            }
        }

        private bool TryHandleCommand(string input)
        {
            if (input.Contains("remind") || input.Contains("reminder"))
            {
                ParseAndSetReminder(input);
                return true;
            }

            if (input.Contains("turn on the light") || input.Contains("lights on"))
            {
                //DeviceController.TurnOnLights();
                Respond("Lights have been turned on.");
                return true;
            }

            if (input.Contains("turn off the light") || input.Contains("lights off"))
            {
                //DeviceController.TurnOffLights();
                Respond("Lights are now off.");
                return true;
            }

            return false;
        }

        private void ParseAndSetReminder(string input)
        {
            var task = "your task";
            var minutes = 1;

            foreach (var word in input.Split(" "))
            {
                if (int.TryParse(word, out var parsed))
                {
                    minutes = parsed;
                    break;
                }
            }

            SetReminder(task, minutes);
            Respond($"Setting reminder for '{task}' in {minutes} minutes.");
        }

        public void SetReminder(string task, int minutes)
        {
            var timer = new Timer(minutes * 60 * 1000);
            timer.Elapsed += (s, e) =>
            {
                VoiceOutput.SpeakAsync($"Reminder: {task}");
                timer.Stop();
            };
            timer.Start();
            _reminders[task] = timer;
            _logger.LogInformation("Reminder set: {Task} in {Minutes} minutes", task, minutes);
        }

        private void Respond(string message)
        {
            _logger.LogInformation("JARVIS: {Message}", message);
            Console.WriteLine($"JARVIS: {message}");
            VoiceOutput.SpeakAsync(message);
        }
    }
}