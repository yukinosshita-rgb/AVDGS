namespace AVDGS.Web.Models.ViewModels
{
    public class ReportsVM
    {
        public string PageTitle { get; set; } = "Flight Docking Reports Dashboard";
        public string PageSubtitle { get; set; } = "";
        public string ReportPeriodLabel { get; set; } = DateTime.Now.ToString("MMMM yyyy");

        public bool HasLiveData { get; set; }
        public string? LoadError { get; set; }

        public int TotalSessions { get; set; }
        public int TotalSuccessful { get; set; }
        public int TotalIncidents { get; set; }
        public double OverallSuccessRate { get; set; }
        public double AvgCenterlineM { get; set; }
        public double AvgDistToStopM { get; set; }

        public List<ReportsRecentRowVM> RecentDockingSessions { get; set; } = new();
        public List<ReportsByAircraftVM> ByAircraft { get; set; } = new();
        public List<ReportsPerfRowVM> PerformanceSummary { get; set; } = new();

        // Charts
        public List<string> ChartAircraftLabels { get; set; } = new();
        public List<int> ChartAircraftSessions { get; set; } = new();
        public List<double> ChartAircraftSuccessRate { get; set; } = new();
        public List<double> ChartAircraftAvgCenterline { get; set; } = new();
        public List<double> ChartAircraftAvgStopDistance { get; set; } = new();
    }

    public class ReportsRecentRowVM
    {
        public string Aircraft { get; set; } = "";
        public DateTime TimestampLocal { get; set; }
        public string Outcome { get; set; } = "Success";
        public double CenterlineM { get; set; }
        public double DistToStopM { get; set; }
        public int IncidentCount { get; set; }

        public string OutcomeCss =>
            Outcome.ToUpperInvariant() switch
            {
                "INCIDENT" => "rptx-chip-bad",
                "FAILED" => "rptx-chip-warn",
                _ => "rptx-chip-good"
            };

        public string OutcomeIcon =>
            Outcome.ToUpperInvariant() switch
            {
                "INCIDENT" => "bi-exclamation-triangle-fill",
                "FAILED" => "bi-x-octagon-fill",
                _ => "bi-check-circle-fill"
            };
    }

    public class ReportsByAircraftVM
    {
        public string Aircraft { get; set; } = "";
        public int Sessions { get; set; }
        public double SuccessRate { get; set; }
        public double AvgOffsetM { get; set; }

        public string SessionsText => $"{Sessions} session{(Sessions == 1 ? "" : "s")}";
        public string PerformanceToneCss =>
            SuccessRate >= 90 ? "rptx-tone-cyan" :
            SuccessRate >= 75 ? "rptx-tone-blue" : "rptx-tone-amber";

        public string VisualHeightPx => $"{Math.Max(120, (int)Math.Round(SuccessRate * 2.2))}px";
    }

    public class ReportsPerfRowVM
    {
        public string Aircraft { get; set; } = "";
        public int Total { get; set; }
        public int Successful { get; set; }
        public int Incidents { get; set; }
        public double SuccessRate { get; set; }
        public double AvgCenterlineM { get; set; }
        public double AvgDistToStopM { get; set; }

        public string PerformanceToneCss =>
            SuccessRate >= 90 ? "rptx-chip-good" :
            SuccessRate >= 75 ? "rptx-chip-info" : "rptx-chip-warn";
    }
}