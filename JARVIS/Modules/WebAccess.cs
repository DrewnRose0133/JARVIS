
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    public static class WebAccess
    {
        public static async Task<string> SearchAsync(string query)
        {
            Logger.Log("Searching web for: " + query);
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                string result = await client.GetStringAsync(url);
                return "Search completed (HTML content not shown)";
            }
        }
    }
}
