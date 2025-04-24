using System;
using System.Linq;
using System.Threading.Tasks;
using KoenZomers.Ring.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules.Devices
{
    /// <summary>
    /// Interface for Ring doorbell motion detection.
    /// </summary>
    public interface IMotionService
    {
        /// <summary>
        /// Checks if the specified doorbell detected motion in its most recent event.
        /// </summary>
        Task<bool> IsMotionDetectedAsync(string doorbotId);
    }

    /// <summary>
    /// Ring-based implementation of IMotionService using KoenZomers.Ring.Api.
    /// </summary>
    public class RingMotionService : IMotionService
    {
        private readonly Session _ringSession;
        private readonly ILogger<RingMotionService> _logger;

        public RingMotionService(
            IConfiguration configuration,
            ILogger<RingMotionService> logger)
        {
            _logger = logger;
            var email = configuration["Ring:Email"];
            var password = configuration["Ring:Password"];
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Ring credentials are not configured.");

            _ringSession = new Session(email, password);
            _ringSession.Authenticate().GetAwaiter().GetResult();
            _logger.LogInformation("Authenticated to Ring account {Email}", email);
        }

        /// <inheritdoc />
        public async Task<bool> IsMotionDetectedAsync(string doorbotId)
        {
            // Retrieve the most recent history event for this doorbell
            var history = await _ringSession.GetDoorbotsHistory(1, doorbotId);
            if (!history.Any())
            {
                _logger.LogWarning("No history events for doorbot {DoorbotId}", doorbotId);
                return false;
            }

            var latest = history.First();
            bool isMotion = string.Equals(latest.Kind, "motion", StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation(
                "Doorbot {DoorbotId} latest event: {Kind}, motion={IsMotion}",
                doorbotId, latest.Kind, isMotion);

            return isMotion;
        }
    }
}
