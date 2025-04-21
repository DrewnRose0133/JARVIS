
using System;

namespace JARVIS.Modules
{
    public static class AudioController
    {
        public static void PlayAudio(string room, string track)
        {
            Logger.Log($"Playing '{track}' in {room}...");
            VoiceOutput.Speak($"Now playing {track} in the {room}");
        }
    }
}
