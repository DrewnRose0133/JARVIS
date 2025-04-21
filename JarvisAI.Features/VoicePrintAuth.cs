
// Simulated voice print authentication
public static class VoicePrintAuth
{
    private static Dictionary<string, string> voicePrints = new Dictionary<string, string>
    {
        { "tony", "print1" }, { "pepper", "print2" }
    };

    public static bool Authenticate(string user, string printSample)
    {
        return voicePrints.ContainsKey(user) && voicePrints[user] == printSample;
    }
}
