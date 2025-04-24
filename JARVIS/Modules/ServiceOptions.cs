namespace JARVIS.Modules
{
    /// <summary>
    /// Configuration options for OpenAI integration, bound from "OpenAI" section in appsettings.json
    /// </summary>
    public class OpenAIOptions
    {
        public string Key { get; set; }
    }

    /// <summary>
    /// Configuration options for Azure Speech integration, bound from "AzureSpeech" section in appsettings.json
    /// </summary>
    public class AzureSpeechOptions
    {
        public string Key { get; set; }
        public string Region { get; set; }
    }
}
