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
    public interface IRingMotionService
    {
        /// <summary>
        /// Checks if the specified doorbell detected motion in its most recent event.
        /// </summary>
        Task<bool> IsMotionDetectedAsync(string doorbotId);
    }

    /// <summary>
    /// Ring-based implementation of IMotionService using KoenZomers.Ring.Api.
    /// </summary>
    public class RingMotionService : IRingMotionService
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
            // Retrieve all history events
            var history = await _ringSession.GetDoorbotsHistory();
            // Filter for the specified doorbot and get the latest event
            var latestEvent = history
                .Where(e => e.Doorbot != null && e.Doorbot.Id.ToString() == doorbotId)
                .OrderByDescending(e => e.CreatedAtDateTime)
                .FirstOrDefault();

            if (latestEvent == null)
            {
                _logger.LogWarning("No history events for doorbot {DoorbotId}", doorbotId);
                return false;
            }

            bool isMotion = string.Equals(latestEvent.Kind, "motion", StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation(
                "Doorbot {DoorbotId} latest event: {Kind}, motion={IsMotion}",
                doorbotId, latestEvent.Kind, isMotion);

            return isMotion;
        }
    }
}