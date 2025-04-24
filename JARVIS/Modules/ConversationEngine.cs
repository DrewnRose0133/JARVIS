using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Handles conversation-related operations such as setting reminders.
    /// </summary>
    public class ConversationEngine
    {
        private readonly ILogger<ConversationEngine> _logger;
        private readonly Dictionary<string, Timer> _reminders = new Dictionary<string, Timer>();

        public ConversationEngine(ILogger<ConversationEngine> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sets a reminder for the specified task in a given number of minutes.
        /// </summary>
        public void SetReminder(string task, int minutes)
        {
            var timer = new Timer(minutes * 60 * 1000);
            timer.Elapsed += (s, e) =>
            {
                // Fire-and-forget async call to speak the reminder
                VoiceOutput.SpeakAsync($"Reminder: {task}");
                timer.Stop();
            };
            timer.Start();
            _reminders[task] = timer;
            _logger.LogInformation("Reminder set: {Task} in {Minutes} minutes", task, minutes);
        }
    }
}