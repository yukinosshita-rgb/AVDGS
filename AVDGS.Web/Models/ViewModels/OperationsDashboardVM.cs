using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AVDGS.Web.Models.ViewModels
{
    public class OperationsDashboardVM
    {
        public string PageTitle { get; set; } = "Flight School Operations Dashboard";
        public string PageSubtitle { get; set; } = "Comprehensive analytics and operational logs";

        // KPI Cards
        public int TotalSessions { get; set; }
        public double SuccessRate { get; set; }
        public int Incidents { get; set; }
        public double AvgDurationMinutes { get; set; }

        // Recent sessions
        public string RecentDockingSessionsTitle { get; set; } = "Recent Docking Sessions";
        public string RecentDockingSessionsSubtitle { get; set; } = "Detailed log of all docking operations";
        public bool HasDockingSessions { get; set; }
        public List<DockingSessionRowVM> RecentSessions { get; set; } = new();

        // ✅ Session Filters (raw strings so dd/MM/yyyy works)
        public string FilterFromRaw { get; set; } = "";
        public string FilterToRaw { get; set; } = "";
        public long? FilterSessionId { get; set; }
        public int? FilterAircraftTypeId { get; set; }
        public int? FilterModeId { get; set; }
        public string FilterOutcome { get; set; } = "";

        // Session Sorting
        public string SortBy { get; set; } = "Start";
        public string SortDir { get; set; } = "DESC";

        // Dropdown options
        public List<SelectListItem> AircraftFilterOptions { get; set; } = new();
        public List<SelectListItem> ModeFilterOptions { get; set; } = new();

        // Success Rate by Aircraft Type
        public List<AircraftSuccessVM> SuccessByAircraft { get; set; } = new();

        // Manual vs Assisted
        public double ManualOverrideSuccess { get; set; }
        public double AvdgsAssistedSuccess { get; set; }

        // ============================
        // ✅ Recent Incident Reports
        // ============================
        public string RecentIncidentReportsTitle { get; set; } = "Recent Incident Reports";
        public string RecentIncidentReportsSubtitle { get; set; } = "Latest safety notes and deviations";

        // Incident filters (separate from session filters)
        public string IncFilterFromRaw { get; set; } = "";
        public string IncFilterToRaw { get; set; } = "";
        public long? IncFilterSessionId { get; set; }
        public string IncFilterSeverity { get; set; } = "";  // "", LOW, MEDIUM, HIGH
        public string IncFilterQuery { get; set; } = "";     // keyword

        public List<IncidentVM> RecentIncidents { get; set; } = new();
    }

    public record AircraftSuccessVM(string AircraftType, double SuccessRate);

    public class DockingSessionRowVM
    {
        public long SessionId { get; set; }
        public string AircraftType { get; set; } = "";
        public string Mode { get; set; } = "";
        public DateTime StartTimeLocal { get; set; }
        public DateTime EndTimeLocal { get; set; }
        public double DurationMinutes { get; set; }
        public bool IsSuccess { get; set; }
        public int IncidentCount { get; set; }

        public string OutcomeText => IsSuccess ? "SUCCESS" : "FAILED";
        public string OutcomeCss => IsSuccess ? "badge bg-success" : "badge bg-danger";
    }

    public class IncidentVM
    {
        public long SessionId { get; set; }
        public string Severity { get; set; } = "LOW";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Today;

        public string SeverityCss =>
            Severity.ToUpper() switch
            {
                "HIGH" => "sev-high",
                "MEDIUM" => "sev-medium",
                _ => "sev-low"
            };
    }
}