
using Microsoft.AspNetCore.Mvc;

namespace JarvisAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvancedController : ControllerBase
    {
        [HttpPost("voice-auth")]
        public IActionResult VoiceAuth([FromBody] AuthRequest req)
        {
            bool result = VoicePrintAuth.Authenticate(req.User, req.VoiceSample);
            return Ok(new { success = result });
        }

        [HttpPost("garage/open")]
        public IActionResult OpenGarage() => Ok(GarageDoorController.Open());

        [HttpPost("garage/close")]
        public IActionResult CloseGarage() => Ok(GarageDoorController.Close());

        [HttpPost("schedule")]
        public IActionResult Schedule([FromBody] ScheduleRequest req)
        {
            DynamicScheduler.ScheduleCommand(req.Time, req.Command);
            return Ok("Scheduled.");
        }

        [HttpPost("panic")]
        public IActionResult Panic() => Ok(EmergencyProtocol.ActivatePanicMode());

        [HttpPost("fire-override")]
        public IActionResult FireOverride() => Ok(EmergencyProtocol.FireAlarmOverride());

        [HttpPost("diagnostics")]
        public IActionResult Diagnostics() => Ok(SelfDiagnostics.RunCheck());

        [HttpPost("personality")]
        public IActionResult SetPersonality([FromQuery] string mode)
        {
            PersonalityEngine.CurrentMode = Enum.Parse<PersonalityEngine.Mode>(mode, true);
            return Ok($"Personality set to {mode}");
        }
    }

    public class AuthRequest
    {
        public string User { get; set; }
        public string VoiceSample { get; set; }
    }

    public class ScheduleRequest
    {
        public DateTime Time { get; set; }
        public string Command { get; set; }
    }
}
