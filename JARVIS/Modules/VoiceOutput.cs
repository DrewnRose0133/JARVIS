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
            Synth.SelectVoice("Microsoft David Desktop");
            Synth.Rate = 0;
            Synth.Volume = 100;
        }

        public static Task SpeakAsync(string text)
        {
            // wrap your text in SSML and escape it
            string ssml = $@"
             <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
               <voice name='{Synth.Voice.Name}'>
                 <prosody rate='-10%' pitch='-6st'>
                   {System.Security.SecurityElement.Escape(text)}
                 </prosody>
               </voice>
             </speak>";
            
                 // SpeakSsml is synchronous, so run it on a thread-pool thread
                 return Task.Run(() => Synth.SpeakSsml(ssml));
        }
    }
}