using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Schedules and executes delayed tasks, including voice reminders.
    /// </summary>
    public class Scheduler
    {
        private readonly List<Timer> _scheduledTimers = new List<Timer>();
        private readonly PersonalityEngine _personalityEngine;
        private readonly ILogger<Scheduler> _logger;

        public Scheduler(
            PersonalityEngine personalityEngine,
            ILogger<Scheduler> logger)
        {
            _personalityEngine = personalityEngine;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the scheduler and sets up any default reminders.
        /// </summary>
        public void Start()
        {
            _logger.LogInformation("Scheduler initialized.");
            // Example: schedule a reminder in 10 minutes
            ScheduleAction(TimeSpan.FromMinutes(10), async () =>
            {
                await _personalityEngine.Speak("Reminder: check the garage.");
            });
        }

        /// <summary>
        /// Schedules an asynchronous action to run after the specified delay.
        /// </summary>
        /// <param name="delay">Delay before execution.</param>
        /// <param name="action">Async action to execute.</param>
        public void ScheduleAction(TimeSpan delay, Func<Task> action)
        {
            var timer = new Timer(delay.TotalMilliseconds);
            timer.Elapsed += async (s, e) =>
            {
                await action();
                timer.Stop();
                _scheduledTimers.Remove(timer);
            };
            timer.Start();
            _scheduledTimers.Add(timer);
            _logger.LogInformation("Scheduled task in {Minutes} minute(s)", delay.TotalMinutes);
        }

        /// <summary>
        /// Stops all scheduled timers and clears the queue.
        /// </summary>
        public void StopAll()
        {
            foreach (var timer in _scheduledTimers)
            {
                timer.Stop();
            }
            _scheduledTimers.Clear();
        }
    }
}
