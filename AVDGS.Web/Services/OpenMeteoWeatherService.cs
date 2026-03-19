using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AVDGS.Web.Models;
using AVDGS.Web.Options;

namespace AVDGS.Web.Services
{
    public sealed class OpenMeteoWeatherService : IWeatherService
    {
        private readonly HttpClient _http;
        private readonly WeatherOptions _opt;
        private readonly ILogger<OpenMeteoWeatherService> _log;

        public OpenMeteoWeatherService(
            HttpClient http,
            IOptions<WeatherOptions> options,
            ILogger<OpenMeteoWeatherService> log)
        {
            _http = http;
            _opt = options.Value;
            _log = log;

            _http.BaseAddress ??= new Uri("https://api.open-meteo.com/"); 
            _http.Timeout = TimeSpan.FromSeconds(3);
        }

        public async Task<WeatherSnapshot?> GetCurrentAsync(CancellationToken ct = default)
        {
            try
            {
                
                var url =
                    $"v1/forecast" +
                    $"?latitude={_opt.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                    $"&longitude={_opt.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                    $"&current=temperature_2m,precipitation,rain,showers,weather_code,wind_speed_10m,wind_gusts_10m,wind_direction_10m,visibility" +
                    $"&timezone={Uri.EscapeDataString(_opt.Timezone)}" +
                    $"&wind_speed_unit=kn";

                using var resp = await _http.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    _log.LogWarning("Open-Meteo weather call failed: {Status}", resp.StatusCode);
                    return null;
                }

                var json = await resp.Content.ReadAsStringAsync(ct);

                var model = JsonSerializer.Deserialize<OpenMeteoResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var cur = model?.Current;
                if (cur == null) return null;

                DateTime? observed = null;
                if (!string.IsNullOrWhiteSpace(cur.Time) &&
                    DateTime.TryParse(cur.Time, out var dt))
                {
                    observed = dt;
                }

                var visibilityKm = cur.VisibilityMeters.HasValue ? (cur.VisibilityMeters.Value / 1000.0) : 0;

                // Lightning: thunderstorm codes 95/96/99
                var wc = cur.WeatherCode ?? 0;
                var isLightning = wc >= 95;

                // Rain: any measurable precipitation/rain/showers
                var isRaining =
                    (cur.PrecipitationMm ?? 0) > 0 ||
                    (cur.RainMm ?? 0) > 0 ||
                    (cur.ShowersMm ?? 0) > 0;

                var windDirDeg = (int)Math.Round(cur.WindDirectionDeg ?? 0);
                var windDirText = ToCompass16(windDirDeg);

                return new WeatherSnapshot
                {
                    Location = _opt.LocationLabel,
                    ObservedAtLocal = observed,

                    VisibilityKm = Math.Round(visibilityKm, 1),
                    TemperatureC = (int)Math.Round(cur.TemperatureC ?? 0),

                    WindSpeedKts = (int)Math.Round(cur.WindSpeedKnots ?? 0),
                    WindGustKts = (int)Math.Round(cur.WindGustKnots ?? 0),

                    // ✅ NEW
                    WindDirectionDeg = windDirDeg,
                    WindDirectionText = windDirText,

                    IsRaining = isRaining,
                    IsLightning = isLightning
                };
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Open-Meteo weather call exception");
                return null;
            }
        }

        // 16-point compass (N, NNE, NE, ..., NNW)
        private static string ToCompass16(int degrees)
        {
            degrees = ((degrees % 360) + 360) % 360; // normalize 0-359
            string[] dirs = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
            int idx = (int)Math.Round(degrees / 22.5) % 16;
            return dirs[idx];
        }

        // -------- DTOs for Open-Meteo JSON --------
        private sealed class OpenMeteoResponse
        {
            [JsonPropertyName("current")]
            public OpenMeteoCurrent? Current { get; set; }
        }

        private sealed class OpenMeteoCurrent
        {
            [JsonPropertyName("time")]
            public string? Time { get; set; }

            [JsonPropertyName("temperature_2m")]
            public double? TemperatureC { get; set; }

            [JsonPropertyName("wind_speed_10m")]
            public double? WindSpeedKnots { get; set; }

            [JsonPropertyName("wind_gusts_10m")]
            public double? WindGustKnots { get; set; }

            [JsonPropertyName("wind_direction_10m")]
            public double? WindDirectionDeg { get; set; }

            [JsonPropertyName("visibility")]
            public double? VisibilityMeters { get; set; }

            [JsonPropertyName("precipitation")]
            public double? PrecipitationMm { get; set; }

            [JsonPropertyName("rain")]
            public double? RainMm { get; set; }

            [JsonPropertyName("showers")]
            public double? ShowersMm { get; set; }

            [JsonPropertyName("weather_code")]
            public int? WeatherCode { get; set; }
        }
    }
}