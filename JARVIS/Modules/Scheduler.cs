using System;
using System.Collections.Generic;
using System.Timers;

namespace JARVIS.Modules
{
    public static class Scheduler
    {
        private static List<Timer> scheduledTimers = new List<Timer>();

        public static void Start()
        {
            Logger.Log("Scheduler initialized.");

            // Example: schedule a reminder
            ScheduleAction(TimeSpan.FromMinutes(10), () =>
            {
                PersonalityEngine.Speak("Reminder: check the garage.");
            });
        }

        public static void ScheduleAction(TimeSpan delay, Action action)
        {
            Timer timer = new Timer(delay.TotalMilliseconds);
            timer.Elapsed += (s, e) =>
            {
                action();
                timer.Stop();
                scheduledTimers.Remove(timer);
            };
            timer.Start();
            scheduledTimers.Add(timer);
            Logger.Log($"Scheduled task in {delay.TotalMinutes} minute(s).");
        }

        public static void StopAll()
        {
            foreach (var timer in scheduledTimers)
                timer.Stop();
            scheduledTimers.Clear();
        }
    }
}