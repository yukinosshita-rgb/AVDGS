namespace AVDGS.Web.Models.ViewModels
{
    public class SettingsVM
    {
        // CAMERA
        public int Brightness { get; set; } = 55;
        public int Exposure { get; set; } = 45;
        public int Contrast { get; set; } = 55;
        public int Saturation { get; set; } = 50;

        public bool AutoWhiteBalance { get; set; } = true;
        public bool HDRMode { get; set; } = false;
        public bool NightVision { get; set; } = true;

        // AIRCRAFT
        public bool HasAircraftPresets { get; set; } = false;

        // ZONES
        public int ZoneWidthM { get; set; } = 45;
        public int ZoneLengthM { get; set; } = 60;
        public double CenterlineToleranceM { get; set; } = 0.5;
        public int StopPositionM { get; set; } = 15;

        public bool ShowCenterline { get; set; } = true;
        public bool ShowDistanceMarkers { get; set; } = true;
        public bool ShowBoundaryLines { get; set; } = true;

        // SAFETY
        public int MaxApproachSpeedKmh { get; set; } = 25;
        public double MaxCenterlineDeviationM { get; set; } = 1.0;
        public int MinAiConfidencePct { get; set; } = 85;

        public bool EmergencyStopEnabled { get; set; } = true;
        public bool AudioWarnings { get; set; } = true;
        public bool ManualOverrideAllowed { get; set; } = true;

        // DATA
        public bool AutomaticDailyBackup { get; set; } = true;
        public bool IncludeCameraFootage { get; set; } = false;

        public string ExportFormat { get; set; } = "CSV"; // CSV/JSON/PDF

        public static SettingsVM BuildSample() => new SettingsVM();
    }
}
