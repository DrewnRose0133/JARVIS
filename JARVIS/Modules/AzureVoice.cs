
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JARVIS.Modules
{
    public static class AzureVoice
    {
        private static readonly string subscriptionKey = File.ReadAllText("Config/azure_key.txt").Trim();
        private static readonly string region = File.ReadAllText("Config/azure_region.txt").Trim();
        private static readonly string endpoint = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";

        public static void Speak(string text)
        {
            try
            {
                var result = SynthesizeSpeechAsync(text).GetAwaiter().GetResult();
                string tempFile = Path.GetTempFileName() + ".wav";
                File.WriteAllBytes(tempFile, result);
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(tempFile);
                player.PlaySync();
                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                Logger.Log("Azure TTS failed, using fallback: " + ex.Message);
                VoiceOutput.Speak(text);
            }
        }

        private static async Task<byte[]> SynthesizeSpeechAsync(string text)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                client.DefaultRequestHeaders.Add("User-Agent", "JARVIS-AI");

                var body = $@"
<speak version='1.0' xml:lang='en-GB'>
  <voice xml:lang='en-GB' xml:gender='Male' name='en-GB-RyanNeural'>
    {text}
  </voice>
</speak>";

                using (var content = new StringContent(body, Encoding.UTF8, "application/ssml+xml"))
                {
                    content.Headers.Add("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");
                    var response = await client.PostAsync(endpoint, content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
        }
    }
}
