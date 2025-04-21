
using System;
using System.Collections.Generic;
using System.Timers;

namespace JARVIS.Modules
{
    public static class ConversationEngine
    {
        private static Dictionary<string, Timer> reminders = new Dictionary<string, Timer>();

        public static void SetReminder(string task, int minutes)
        {
            Timer timer = new Timer(minutes * 60 * 1000);
            timer.Elapsed += (s, e) => {
                VoiceOutput.Speak($"Reminder: {task}");
                timer.Stop();
            };
            timer.Start();
            reminders[task] = timer;
            Logger.Log($"Reminder set: {task} in {minutes} minutes");
        }
    }
}
