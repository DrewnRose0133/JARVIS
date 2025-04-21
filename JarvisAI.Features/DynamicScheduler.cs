
// Schedule command execution (can be upgraded to cron or NLP later)
public static class DynamicScheduler
{
    private static List<(DateTime time, string command)> tasks = new List<(DateTime, string)>();

    public static void ScheduleCommand(DateTime time, string command)
    {
        tasks.Add((time, command));
        Console.WriteLine($"Scheduled '{command}' at {time}");
    }

    public static void RunPending()
    {
        var now = DateTime.Now;
        foreach (var task in tasks.Where(t => t.time <= now).ToList())
        {
            CommandProcessor.Handle(task.command);
            tasks.Remove(task);
        }
    }
}
