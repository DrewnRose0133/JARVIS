using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using JARVIS.Modules.Devices.Interfaces;
using KoenZomers.Ring.Api;
using KoenZomers.Ring.Api.Exceptions;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules.Devices
{
    /// <summary>
    /// Ring-based implementation of ICameraService using KoenZomers.Ring.Api and Video on Demand.
    /// </summary>
    public class RingCameraService : ICameraService
    {
        private readonly Session _session;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RingCameraService> _logger;

        public RingCameraService(
            Session session,
            IHttpClientFactory httpClientFactory,
            ILogger<RingCameraService> logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("RingClient");
        }

        public async Task<string> TakeSnapshotAsync(string cameraId)
        {
            try
            {
                var devices = await _session.GetRingDevices();
                var doorbot = devices.Doorbots.FirstOrDefault(d => d.Id.ToString() == cameraId);
                if (doorbot == null)
                {
                    _logger.LogWarning("Camera {CameraId} not found.", cameraId);
                    return null;
                }

                var tempFile = Path.Combine(Path.GetTempPath(), $"ring_{cameraId}_snapshot.jpg");
                await _session.GetLatestSnapshot(doorbot.Id, tempFile);
                _logger.LogInformation("Snapshot saved to {Path}", tempFile);
                return tempFile;
            }
            catch (SessionNotAuthenticatedException ex)
            {
                _logger.LogError(ex, "Ring session not authenticated in TakeSnapshotAsync");
                throw new InvalidOperationException("Ring session is not authenticated. Please check credentials.", ex);
            }
        }

        public async Task<string> GetLiveStreamUrlAsync(string cameraId)
        {
            try
            {
                var devices = await _session.GetRingDevices();
                var doorbot = devices.Doorbots.FirstOrDefault(d => d.Id.ToString() == cameraId);
                if (doorbot == null)
                {
                    _logger.LogWarning("Camera {CameraId} not found.", cameraId);
                    return null;
                }

                // Use existing authenticated token
                var token = _session.OAuthToken.AccessToken;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Trigger VOD request
                var vodResp = await _httpClient.PostAsync($"doorbots/{cameraId}/vod", null);
                vodResp.EnsureSuccessStatusCode();
                _logger.LogInformation("Requested VOD for {CameraId}", cameraId);

                // Poll for on_demand event
                var latestEvent = await PollVodEventAsync();

                // Get download URL without redirect
                var dlJson = await _httpClient.GetStringAsync($"dings/{latestEvent.Id}/share/download?disable_redirect=true");
                var dlObj = JsonSerializer.Deserialize<DownloadResponse>(dlJson);
                return dlObj.Url;
            }
            catch (SessionNotAuthenticatedException ex)
            {
                _logger.LogError(ex, "Ring session not authenticated in GetLiveStreamUrlAsync");
                throw new InvalidOperationException("Ring session is not authenticated. Please check credentials.", ex);
            }
        }

        private async Task<VodHistoryEvent> PollVodEventAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                var histJson = await _httpClient.GetStringAsync("doorbots/history?limit=1");
                var history = JsonSerializer.Deserialize<DoorbotHistoryResponse>(histJson);
                if (history?.Events?.Count > 0 && history.Events[0].Kind == "on_demand")
                {
                    return history.Events[0];
                }
            }
            throw new TimeoutException("Timed out waiting for VOD event.");
        }

        private class DoorbotHistoryResponse
        {
            [JsonPropertyName("items")]
            public List<VodHistoryEvent> Events { get; set; }
        }

        private class VodHistoryEvent
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