
// Panic and fire override systems
public static class EmergencyProtocol
{
    public static void ActivatePanicMode()
    {
        Console.WriteLine("Panic mode activated! Locking down system.");
        // Lock doors, notify users, etc.
    }

    public static void FireAlarmOverride()
    {
        Console.WriteLine("Fire alarm override triggered. Disabling alerts temporarily.");
        // Suppress triggers for fire alarm testing
    }
}
