using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JARVIS.Modules.Devices.Interfaces;
using KoenZomers.Ring.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules.Devices
{
    /// <summary>
    /// Ring-based implementation of ICameraService using KoenZomers.Ring.Api.
    /// </summary>
    public class RingCameraService : ICameraService
    {
        private readonly ILogger<RingCameraService> _logger;
        private readonly Session _ringSession;

        public RingCameraService(
            IConfiguration configuration,
            ILogger<RingCameraService> logger)
        {
            _logger = logger;

            var email = configuration["Ring:Email"];
            var password = configuration["Ring:Password"];
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Ring credentials are not configured.");
            }

            _ringSession = new Session(email, password);
            try
            {
                _ringSession.Authenticate().GetAwaiter().GetResult();
                _logger.LogInformation("Authenticated to Ring account {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate to Ring API for {Email}", email);
                throw;
            }
        }

        public Task<string> GetLiveStreamUrlAsync(string cameraId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<string> TakeSnapshotAsync(string cameraId)
        {
            // Retrieve all Ring devices (doorbells and chimes)
            var devices = await _ringSession.GetRingDevices();
            // Select from the Doorbots (Ring doorbells)
            var doorbots = devices.Doorbots;
            var device = doorbots.FirstOrDefault(d => d.Id.ToString() == cameraId);
            if (device == null)
            {
                _logger.LogWarning("Ring device {CameraId} not found among Doorbots.", cameraId);
                return null;
            }

            // Define a local path for the snapshot
            var tmpFile = Path.Combine(
                Path.GetTempPath(),
                $"ring_{cameraId}_snapshot.jpg");

            // Download the latest snapshot
            await _ringSession.GetLatestSnapshot(device.Id, tmpFile);
            _logger.LogInformation("Snapshot saved to {Path}", tmpFile);
            return tmpFile;
        }
    }
}