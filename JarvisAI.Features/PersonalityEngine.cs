
// Dynamic responses based on personality mode
public static class PersonalityEngine
{
    public enum Mode { Formal, Witty, Casual }
    public static Mode CurrentMode = Mode.Casual;

    public static string Respond(string input)
    {
        return CurrentMode switch
        {
            Mode.Formal => $"Affirmative. {input}",
            Mode.Witty => $"Well, aren't you clever! {input}",
            Mode.Casual => $"Got it. {input}",
            _ => input
        };
    }
}
