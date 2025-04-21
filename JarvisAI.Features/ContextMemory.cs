
// Contextual memory for short-term interaction tracking
public static class ContextMemory
{
    private static Dictionary<string, string> memory = new();

    public static void Remember(string key, string value) => memory[key] = value;

    public static string Recall(string key) => memory.TryGetValue(key, out var value) ? value : "Nothing found.";
}
