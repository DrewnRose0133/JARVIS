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
            Synth.SelectVoice("Microsoft David Desktop");
            Synth.Rate = 0;
            Synth.Volume = 100;
        }

        public static void SpeakAsync(string text)
        {
            string ssml = $@"
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
              <voice name='Microsoft David Desktop'>
                <prosody rate='-10%' pitch='-4st'>
                  {System.Security.SecurityElement.Escape(text)}
                </prosody>
              </voice>
            </speak>";

            Synth.SpeakSsmlAsync(ssml);
        }
    }
}