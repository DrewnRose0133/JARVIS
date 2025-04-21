
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    public static class OpenAIClient
    {
        private static readonly string apiKey = System.IO.File.ReadAllText("Config/openai_key.txt").Trim();

        public static async Task<string> Ask(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                string json = JsonSerializer.Serialize(requestBody);
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                string result = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(result))
                {
                    return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").ToString();
                }
            }
        }
    }
}
