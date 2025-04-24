using System.Net.NetworkInformation;
using System.Security;
using System.Speech.Synthesis;
using System;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    public static class VoiceOutput
    {
        private static readonly SpeechSynthesizer Synth = new SpeechSynthesizer();


        static VoiceOutput()
        {
            Synth.SelectVoice("Microsoft Hazel Desktop");
            Synth.Rate = 0;
            Synth.Volume = 100;
        }

        public static Task SpeakAsync(string text)
        {
            string ssml = $@"
                <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
                  <voice name='Microsoft Hazel Desktop'>
                    <!-- slow way down and drop the pitch way down -->
                    <prosody rate='-20%' pitch='-50st'>
                      <!-- add a pause before starting for dramatic effect -->
                      <break time='300ms'/>
                      {System.Security.SecurityElement.Escape(text)}
                      <!-- add a little trailing pause -->
                      <break time='200ms'/>
                     <emphasis level=""moderate"">Good morning, sir.</emphasis>
                    </prosody>
                  </voice>
                </speak>";

            // SpeakSsml is synchronous, so run it on a thread-pool thread
            return Task.Run(() => Synth.SpeakSsml(ssml));
        }
    }
}