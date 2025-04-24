using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    public class PersonalityEngine
    {
        public enum Personality { Professional, Friendly, Sarcastic, Jarvis }

        private readonly ILogger<PersonalityEngine> _logger;
        public Personality Current { get; private set; } = Personality.Jarvis;

        public PersonalityEngine(ILogger<PersonalityEngine> logger)
        {
            _logger = logger;
        }

        public async Task Speak(string message)
        {
            switch (Current)
            {
                case Personality.Jarvis:
                    // dry, witty JARVIS style
                    await JARVISMode.Speak(message);
                    break;

                case Personality.Sarcastic:
                    await VoiceOutput.SpeakAsync("Oh, really? " + message);
                    break;

                case Personality.Professional:
                    await VoiceOutput.SpeakAsync("System response: " + message);
                    break;

                default:
                    // Friendly or default fallback
                    await VoiceOutput.SpeakAsync(message);
                    break;
            }
        }

        public async Task SetPersonality(string mode)
        {
            if (Enum.TryParse<Personality>(mode, true, out var newMode))
            {
                Current = newMode;
                _logger.LogInformation("Personality set to {Personality}", Current);
                await VoiceOutput.SpeakAsync($"Personality set to {Current}");
            }
            else
            {
                _logger.LogWarning("Invalid personality: {Mode}", mode);
                await VoiceOutput.SpeakAsync("That personality isn't available.");
            }
        }
    }
}
