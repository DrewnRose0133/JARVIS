using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    public class AudioController
    {
        private readonly ILogger<AudioController> _logger;

        public AudioController(ILogger<AudioController> logger)
        {
            _logger = logger;
        }

        public async Task PlayAudio(string room, string track)
        {
            _logger.LogInformation("Playing '{Track}' in {Room}", track, room);
            await VoiceOutput.SpeakAsync($"Now playing {track} in the {room}");
        }
    }
}
