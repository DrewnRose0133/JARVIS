
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    public static class UniFiController
    {
        private static readonly string controllerUrl = System.IO.File.ReadAllText("Config/unifi_config.json").Split('\n')[0].Split(':')[1].Trim();
        private static readonly string apiToken = System.IO.File.ReadAllText("Config/unifi_config.json").Split('\n')[1].Split(':')[1].Trim();

        public static async Task GetConnectedDevices()
        {
            Logger.Log("Requesting list of connected devices from UniFi...");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(controllerUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                    var response = await client.GetAsync("/proxy/network/api/s/default/stat/sta");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    Logger.Log("Connected devices:
" + json.Substring(0, Math.Min(json.Length, 500)) + "...");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("UniFi error: " + ex.Message);
            }
        }

        public static async Task RestartRouter()
        {
            Logger.Log("Restarting UniFi router (placeholder)");
            // Real restart may require SSH or advanced endpoint
            await Task.CompletedTask;
        }
    }
}
