using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JARVIS.Modules
{
    public class OpenAIClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<OpenAIClient> _logger;

        public OpenAIClient(
            HttpClient http,
            IOptions<OpenAIOptions> opts,
            ILogger<OpenAIClient> logger)
        {
            _http = http;
            _logger = logger;
            _http.BaseAddress = new Uri("https://api.openai.com/v1/");
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", opts.Value.Key);
        }

        public async Task<string> Ask(string prompt)
        {
            // system prompt for J.A.R.V.I.S. persona
            const string systemPrompt =
                "You are J.A.R.V.I.S., Tony Stark’s AI assistant. " +
                "Speak in a dry, witty, slightly British tone—concise, clever, " +
                "and with a hint of polite sarcasm, in the style of Paul Bettany.";

            var payload = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var answer = doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString();

            _logger.LogInformation("OpenAI → {Answer}", answer);
            return answer;
        }
    }
}
