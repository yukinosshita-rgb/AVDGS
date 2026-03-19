using Microsoft.AspNetCore.Mvc;
using AVDGS.Web.Models.ViewModels;
using AVDGS.Web.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AVDGS.Web.Controllers
{
    public class DockingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWeatherService _weather;

        public DockingController(IHttpClientFactory httpClientFactory, IWeatherService weather)
        {
            _httpClientFactory = httpClientFactory;
            _weather = weather;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Docking";
            ViewData["ActiveTab"] = "Docking";
            return View();
        }

        public async Task<IActionResult> Live()
        {
            ViewData["Title"] = "Docking";
            ViewData["ActiveTab"] = "Docking";

            var streamUrl = "http://127.0.0.1:5001/video";
            var healthUrl = "http://127.0.0.1:5001/health";

            var isPythonUp = await IsPythonHealthyAsync(healthUrl);

            ViewData["StreamUrl"] = streamUrl;

            var wx = await _weather.GetCurrentAsync();

            var vm = new LiveDockingVM
            {
                PageTitle = "Live Docking Guidance Dashboard",
                PageSubtitle = "Real-time aircraft docking assistance",

                CameraStatusText = isPythonUp ? "CONNECTING" : "OFFLINE",
                CameraIsLive = false,
                CameraFeedImageUrl = "/images/mock/live-camera.png",

                SelectedAircraftType = "Cessna 172",
                AircraftTypes = new List<string> { "Cessna 172", "Cessna 152" },

                EnvironmentLocation = wx?.Location ?? "Lingayen Airport (RPUG), Pangasinan",
                WeatherObservedAtLocal = wx?.ObservedAtLocal,

                VisibilityKm = wx?.VisibilityKm ?? 10,
                WindSpeedKts = wx?.WindSpeedKts ?? 5,
                WindGustKts = wx?.WindGustKts ?? 8,
                TemperatureC = wx?.TemperatureC ?? 32,

                // ✅ NEW
                WindDirectionDeg = wx?.WindDirectionDeg ?? 0,
                WindDirectionText = wx?.WindDirectionText ?? "—",

                IsRaining = wx?.IsRaining ?? false,
                IsLightning = wx?.IsLightning ?? false,

                AIDetectionConfidence = 98.5,
                AIAlignmentConfidence = 95.2,

                CenterlineOffsetMeters = 0.0,
                DistanceToStopMeters = 0.0,
                LateralAdjustmentMeters = 0.0,
                LateralDirection = "Move left",

                GuidanceText = "CENTERED",
                GuidanceDistanceText = "15.2m to stop"
            };

            return View(vm);
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Weather()
        {
            var wx = await _weather.GetCurrentAsync();

            if (wx == null)
                return Json(new { ok = false });

            return Json(new
            {
                ok = true,
                location = wx.Location,
                observedAt = wx.ObservedAtLocal?.ToString("yyyy-MM-dd HH:mm"),
                visibilityKm = wx.VisibilityKm,
                temperatureC = wx.TemperatureC,
                windSpeedKts = wx.WindSpeedKts,
                windGustKts = wx.WindGustKts,

                // ✅ NEW
                windDirectionDeg = wx.WindDirectionDeg,
                windDirectionText = wx.WindDirectionText,

                isRaining = wx.IsRaining,
                isLightning = wx.IsLightning
            });
        }

        private async Task<bool> IsPythonHealthyAsync(string healthUrl)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
                var client = _httpClientFactory.CreateClient();

                var resp = await client.GetAsync(healthUrl, cts.Token);
                if (!resp.IsSuccessStatusCode) return false;

                var body = (await resp.Content.ReadAsStringAsync()).Trim();
                return body.Equals("OK", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}