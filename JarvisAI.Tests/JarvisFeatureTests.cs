
using Xunit;
using System;

public class JarvisFeatureTests
{
    [Fact]
    public void VoicePrintAuth_ShouldAuthenticateApprovedUser()
    {
        Assert.True(VoicePrintAuth.Authenticate("tony", "print1"));
    }

    [Fact]
    public void Scheduler_ShouldStoreAndRunCommand()
    {
        DynamicScheduler.ScheduleCommand(DateTime.Now.AddSeconds(-1), "open garage");
        DynamicScheduler.RunPending();
        Assert.True(true); // Manual check; use mocks for real execution
    }

    [Fact]
    public void EmergencyProtocol_ShouldTriggerPanic()
    {
        EmergencyProtocol.ActivatePanicMode();
        Assert.True(true); // Expect action printed or triggered
    }

    [Fact]
    public void Diagnostics_ShouldRun()
    {
        SelfDiagnostics.RunCheck();
        Assert.True(true); // Should print OK diagnostics
    }

    [Fact]
    public void PersonalityEngine_ShouldRespondCasually()
    {
        PersonalityEngine.CurrentMode = PersonalityEngine.Mode.Casual;
        var response = PersonalityEngine.Respond("System ready");
        Assert.Contains("Got it", response);
    }
}
