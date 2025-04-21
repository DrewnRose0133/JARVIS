using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NAudio.Wave;

namespace JARVIS.Modules
{
    public static class AzureVoice
    {
        private static readonly string azureApiKey = System.IO.File.ReadAllText("Config/azure_key.txt").Trim();
        private static readonly string endpoint = "https://eastus.tts.speech.microsoft.com/cognitiveservices/v1";

        public static async Task Speak(string text)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureApiKey);
            client.DefaultRequestHeaders.Add("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
            client.DefaultRequestHeaders.Add("User-Agent", "JARVIS");

            string body = $@"
<speak version='1.0' xml:lang='en-US'>
    <voice name='en-US-GuyNeural'>{text}</voice>
</speak>";

            var content = new StringContent(body, Encoding.UTF8, "application/ssml+xml");
            var response = await client.PostAsync(endpoint, content);
            var audioBytes = await response.Content.ReadAsByteArrayAsync();

            PlayAudioStream(audioBytes);
        }

        private static void PlayAudioStream(byte[] audioBytes)
        {
            using var ms = new MemoryStream(audioBytes);
            using var rdr = new Mp3FileReader(ms);
            using var waveOut = new WaveOutEvent();
            waveOut.Init(rdr);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}