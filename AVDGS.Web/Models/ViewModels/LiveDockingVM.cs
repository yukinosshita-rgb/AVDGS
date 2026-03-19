using System;
using System.Collections.Generic;

namespace AVDGS.Web.Models.ViewModels
{
    public class LiveDockingVM
    {
        public bool PythonOnline { get; set; }
        public string PythonBaseUrl { get; set; } = "http://127.0.0.1:5001/video";
        public string? PythonError { get; set; }

        public string PageTitle { get; set; } = "Live Docking Guidance Dashboard";
        public string PageSubtitle { get; set; } = "Real-time aircraft docking assistance";

        // Camera card
        public bool CameraIsLive { get; set; } = true;
        public string CameraStatusText { get; set; } = "LIVE";
        public string CameraFeedImageUrl { get; set; } = "/images/mock/live-camera.png";

        // Aircraft selection
        public List<string> AircraftTypes { get; set; } = new();
        public string SelectedAircraftType { get; set; } = "Cessna 172";

        // ✅ Environment (LIVE)
        public string EnvironmentLocation { get; set; } = "Lingayen Airport (RPUG), Pangasinan";
        public DateTime? WeatherObservedAtLocal { get; set; }

        public double VisibilityKm { get; set; }
        public int TemperatureC { get; set; }
        public int WindSpeedKts { get; set; }
        public int WindGustKts { get; set; }

        // ✅ NEW: Wind Direction
        public int WindDirectionDeg { get; set; }            // e.g., 330
        public string WindDirectionText { get; set; } = "—"; // e.g., NNW

        public bool IsRaining { get; set; }
        public bool IsLightning { get; set; }

        // AI confidence
        public double AIDetectionConfidence { get; set; } = 98.5;
        public double AIAlignmentConfidence { get; set; } = 95.2;

        // Docking indicators (real-time)
        public double CenterlineOffsetMeters { get; set; } = 0.2;
        public double DistanceToStopMeters { get; set; } = 15.2;
        public double LateralAdjustmentMeters { get; set; } = 0.5;
        public string LateralDirection { get; set; } = "Move left";

        // Overlay text (optional)
        public string GuidanceText { get; set; } = "CENTERED";
        public string GuidanceDistanceText { get; set; } = "15.2m to stop";
    }
}