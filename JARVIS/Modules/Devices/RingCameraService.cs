using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using JARVIS.Modules.Devices.Interfaces;
using KoenZomers.Ring.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules.Devices
{
    /// <summary>
    /// Ring-based implementation of ICameraService using KoenZomers.Ring.Api and Video on Demand.
    /// </summary>
    public class RingCameraService : ICameraService
    {
        private readonly ILogger<RingCameraService> _logger;
        private readonly Session _ringSession;
        private readonly HttpClient _http;

        public RingCameraService(
            IConfiguration configuration,
            ILogger<RingCameraService> logger)
        {
            _logger = logger;

            var email = configuration["Ring:Email"];
            var password = configuration["Ring:Password"];
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Ring credentials are not configured.");

            _ringSession = new Session(email, password);
            try
            {
                // Authenticate (blocks once at startup)
                _ringSession.Authenticate().GetAwaiter().GetResult();
                _logger.LogInformation("Authenticated to Ring account {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate to Ring API for {Email}", email);
                throw;
            }

            // HttpClient for VOD endpoints
            _http = new HttpClient
            {
                BaseAddress = new Uri("https://api.ring.com/clients_api/")
            };
        }

        /// <inheritdoc />
        public async Task<string> GetLiveStreamUrlAsync(string cameraId)
        {
            // 1) Ensure authentication (in case token expired)
            await _ringSession.Authenticate();

            // 2) Set Bearer token on HttpClient
            var token = _ringSession.OAuthToken.AccessToken;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 3) Request a short VOD clip
            var vodEndpoint = $"doorbots/{cameraId}/vod";
            using var vodResp = await _http.PostAsync(vodEndpoint, content: null);
            vodResp.EnsureSuccessStatusCode();
            _logger.LogInformation("Requested VOD for {CameraId}", cameraId);

            // 4) Poll history for the on_demand event
            VoodHistoryEvent latestEvent = null;
            var historyEndpoint = "doorbots/history?limit=1";
            for (int attempt = 0; attempt < 10; attempt++)
            {
                await Task.Delay(1000);
                var histJson = await _http.GetStringAsync(historyEndpoint);
                var history = JsonSerializer.Deserialize<DoorbotHistoryResponse>(histJson);
                if (history?.Events?.Count > 0 &&
                    history.Events[0].Kind == "on_demand")
                {
                    latestEvent = history.Events[0];
                    break;
                }
            }
            if (latestEvent == null)
                throw new InvalidOperationException("Timed out waiting for VOD clip.");

            _logger.LogInformation("Found VOD event {EventId}", latestEvent.Id);

            // 5) Get the shareable download URL (no redirect)
            var downloadEndpoint = $"dings/{latestEvent.Id}/share/download?disable_redirect=true";
            var dlJson = await _http.GetStringAsync(downloadEndpoint);
            var dlObj = JsonSerializer.Deserialize<DownloadResponse>(dlJson);

            return dlObj.Url;
        }

        /// <inheritdoc />
        public async Task<string> TakeSnapshotAsync(string cameraId)
        {
            // Retrieve all Ring devices (doorbells and chimes)
            var devices = await _ringSession.GetRingDevices();
            var doorbots = devices.Doorbots;
            var device = doorbots.FirstOrDefault(d => d.Id.ToString() == cameraId);

            if (device == null)
            {
                _logger.LogWarning("Ring device {CameraId} not found among Doorbots.", cameraId);
                return null;
            }

            // Download the latest snapshot to a temp file
            var tmpFile = Path.Combine(Path.GetTempPath(), $"ring_{cameraId}_snapshot.jpg");
            await _ringSession.GetLatestSnapshot(device.Id, tmpFile);
            _logger.LogInformation("Snapshot saved to {Path}", tmpFile);

            return tmpFile;
        }

        // --- JSON DTOs for VOD polling and download ---

        private class DoorbotHistoryResponse
        {
            [JsonPropertyName("items")]
            public List<VoodHistoryEvent> Events { get; set; }
        }

        private class VoodHistoryEvent
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("kind")]
            public string Kind { get; set; }
        }

        private class DownloadResponse
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
    }
}
