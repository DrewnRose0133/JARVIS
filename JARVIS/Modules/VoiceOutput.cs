using System.Net.NetworkInformation;
using System.Security;
using System.Speech.Synthesis;
using System;

namespace JARVIS.Modules
{
    public static class VoiceOutput
    {
        private static readonly SpeechSynthesizer Synth = new SpeechSynthesizer();

        static VoiceOutput()
        {
            // List installed voices once to pick a British male voice:
             //foreach(var v in Synth.GetInstalledVoices()) 
               //  Console.WriteLine(v.VoiceInfo.Name);

            // On Windows 10/11 you’ll often see "Microsoft George Desktop" or similar
            Synth.SelectVoice("Microsoft George Desktop");
            Synth.Rate = 0;    // keep default speaking rate
            Synth.Volume = 100;  // max volume
        }

        /// <summary>
        /// Speak text asynchronously using SSML prosody tweaks for a more “Bettany-like” tone.
        /// </summary>
        public static void SpeakAsync(string text)
        {
            // wrap your text in SSML: slower rate, lower pitch
            string ssml = $@"
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
              <voice name='Microsoft George Desktop'>
                <prosody rate='-10%' pitch='-2st'>
                  {SecurityElement.Escape(text)}
                </prosody>
              </voice>
            </speak>";

            Synth.SpeakSsmlAsync(ssml);
        }
    }
}