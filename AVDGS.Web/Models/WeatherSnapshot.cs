using System;

namespace AVDGS.Web.Models
{
    public class WeatherSnapshot
    {
        public string Location { get; set; } = "";
        public DateTime? ObservedAtLocal { get; set; }

        public double VisibilityKm { get; set; }
        public int TemperatureC { get; set; }
        public int WindSpeedKts { get; set; }
        public int WindGustKts { get; set; }
        public int WindDirectionDeg { get; set; }
        public string WindDirectionText { get; set; } = "—";
        public bool IsRaining { get; set; }
        public bool IsLightning { get; set; }
    }
}