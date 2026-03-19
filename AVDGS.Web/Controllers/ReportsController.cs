using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AVDGS_BLL;
using AVDGS.Web.Models.ViewModels;

namespace AVDGS.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ReportsService _svc;

        public ReportsController(ReportsService svc)
        {
            _svc = svc;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Reports";
            ViewData["ActiveTab"] = "Reports";

            var vm = new ReportsVM
            {
                PageTitle = "Flight Docking Reports Dashboard",
                PageSubtitle = "Comprehensive docking analytics, aircraft performance metrics, and visual report summaries for aviation training operations."
            };

            try
            {
                var summary = await _svc.GetSummaryAsync();

                vm.TotalSessions = summary.TotalSessions;
                vm.TotalIncidents = summary.TotalIncidents;
                vm.HasLiveData = summary.TotalSessions > 0;
                vm.ReportPeriodLabel = summary.LastSessionStart?.ToString("MMMM yyyy") ?? DateTime.Now.ToString("MMMM yyyy");

                vm.RecentDockingSessions = (await _svc.GetRecentDockingSessionsAsync(10))
                    .Select(x => new ReportsRecentRowVM
                    {
                        Aircraft = x.AircraftName,
                        TimestampLocal = x.TimestampLocal,
                        Outcome = x.Outcome,
                        CenterlineM = x.CenterlineOffsetM,
                        DistToStopM = x.DistanceToStopM,
                        IncidentCount = x.IncidentCount
                    })
                    .ToList();

                vm.ByAircraft = (await _svc.GetByAircraftTypeAsync())
                    .Select(x => new ReportsByAircraftVM
                    {
                        Aircraft = x.AircraftName,
                        Sessions = x.TotalSessions,
                        SuccessRate = x.SuccessRate,
                        AvgOffsetM = x.AvgOffsetM
                    })
                    .OrderByDescending(x => x.SuccessRate)
                    .ThenByDescending(x => x.Sessions)
                    .ThenBy(x => x.Aircraft)
                    .ToList();

                vm.PerformanceSummary = (await _svc.GetAircraftPerformanceSummaryAsync())
                    .Select(x => new ReportsPerfRowVM
                    {
                        Aircraft = x.AircraftName,
                        Total = x.Total,
                        Successful = x.Successful,
                        Incidents = x.Incidents,
                        SuccessRate = x.SuccessRate,
                        AvgCenterlineM = x.AvgCenterlineM,
                        AvgDistToStopM = x.AvgDistToStopM
                    })
                    .OrderByDescending(x => x.SuccessRate)
                    .ThenByDescending(x => x.Total)
                    .ThenBy(x => x.Aircraft)
                    .ToList();

                vm.TotalSuccessful = vm.PerformanceSummary.Sum(x => x.Successful);
                vm.OverallSuccessRate = vm.TotalSessions == 0
                    ? 0
                    : (double)vm.TotalSuccessful / vm.TotalSessions * 100.0;

                var totalWeightedSessions = vm.PerformanceSummary.Sum(x => x.Total);
                if (totalWeightedSessions > 0)
                {
                    vm.AvgCenterlineM = vm.PerformanceSummary.Sum(x => x.AvgCenterlineM * x.Total) / totalWeightedSessions;
                    vm.AvgDistToStopM = vm.PerformanceSummary.Sum(x => x.AvgDistToStopM * x.Total) / totalWeightedSessions;
                }

                var chartRows = vm.PerformanceSummary.Take(8).ToList();

                vm.ChartAircraftLabels = chartRows.Select(x => x.Aircraft).ToList();
                vm.ChartAircraftSessions = chartRows.Select(x => x.Total).ToList();
                vm.ChartAircraftSuccessRate = chartRows.Select(x => Math.Round(x.SuccessRate, 2)).ToList();
                vm.ChartAircraftAvgCenterline = chartRows.Select(x => Math.Round(x.AvgCenterlineM, 2)).ToList();
                vm.ChartAircraftAvgStopDistance = chartRows.Select(x => Math.Round(x.AvgDistToStopM, 2)).ToList();
            }
            catch
            {
                vm.LoadError = "Failed to load docking report data. Please try again.";
                vm.HasLiveData = false;
            }

            return View(vm);
        }
    }
}