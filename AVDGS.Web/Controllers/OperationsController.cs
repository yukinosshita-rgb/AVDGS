using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AVDGS.Web.Models.ViewModels;
using AVDGS_BLL;
using AVDGS_DAL;
using System.Globalization;

namespace AVDGS.Web.Controllers
{
    public class OperationsController : Controller
    {
        private readonly OperationsService _ops;

        public OperationsController(OperationsService ops)
        {
            _ops = ops;
        }

        // ============================
        // OPERATIONS DASHBOARD
        // ============================
        public async Task<IActionResult> Index(
            string? from,                 // sessions: dd/MM/yyyy or yyyy-MM-dd
            string? to,                   // sessions: dd/MM/yyyy or yyyy-MM-dd (inclusive)
            long? sessionId,              // sessions: #ID
            int? aircraftTypeId,
            int? modeId,
            string? outcome,              // "", "SUCCESS", "FAILED"
            string? sortBy,               // Session/Aircraft/Mode/Start/End/Duration/Outcome
            string? sortDir,              // ASC/DESC

            // ✅ incidents (separate presets)
            string? incFrom,              // incidents: dd/MM/yyyy or yyyy-MM-dd
            string? incTo,                // incidents: inclusive
            long? incSessionId,
            string? incSeverity,          // "", LOW/MEDIUM/HIGH
            string? incQ                  // keyword
        )
        {
            ViewData["Title"] = "Operations";
            ViewData["ActiveTab"] = "Operations";

            // Normalize sort
            sortBy = string.IsNullOrWhiteSpace(sortBy) ? "Start" : sortBy.Trim();
            sortDir = (sortDir?.ToUpper() == "ASC") ? "ASC" : "DESC";

            // Parse session dates
            DateTime? fromDate = ParseDateFlexible(from);
            DateTime? toDate = ParseDateFlexible(to);
            DateTime? toExclusive = toDate?.Date.AddDays(1);

            // Outcome mapping
            bool? outcomeBool = outcome?.ToUpper() switch
            {
                "SUCCESS" => true,
                "FAILED" => false,
                _ => null
            };

            // Global KPIs
            var kpis = await _ops.GetKpisAsync();
            var byAircraft = await _ops.GetSuccessByAircraftAsync();
            var byMode = await _ops.GetSuccessByModeAsync();

            // Sessions table
            var sessions = await _ops.QuerySessionsAsync(
                topN: 50,
                fromDate: fromDate?.Date,
                toDateExclusive: toExclusive,
                sessionId: sessionId,
                aircraftTypeId: aircraftTypeId,
                modeId: modeId,
                outcome: outcomeBool,
                sortBy: sortBy,
                sortDir: sortDir
            );

            // ✅ Incident filters + Top 5
            DateTime? incFromDate = ParseDateFlexible(incFrom);
            DateTime? incToDate = ParseDateFlexible(incTo);
            DateTime? incToExclusive = incToDate?.Date.AddDays(1);

            var incidents = await _ops.QueryIncidentsAsync(
                topN: 5,
                fromDate: incFromDate?.Date,
                toDateExclusive: incToExclusive,
                sessionId: incSessionId,
                severity: incSeverity,
                q: incQ
            );

            // Force aircraft display order (show all 4 even if 0 sessions)
            var aircraftOrder = new[] { "Cessna 152", "Cessna 172", "Piper Aztec", "Seminole" };
            var aircraftMap = byAircraft.ToDictionary(x => x.AircraftName, x => x.SuccessRate);

            // Filter dropdown data
            var aircraftLookups = await _ops.ListAircraftTypesAsync();
            var modeLookups = await _ops.ListOperationModesAsync();

            var vm = new OperationsDashboardVM
            {
                PageTitle = "Flight School Operations Dashboard",
                PageSubtitle = "Comprehensive analytics and operational logs",

                TotalSessions = kpis.TotalSessions,
                SuccessRate = kpis.SuccessRate,
                Incidents = kpis.Incidents,
                AvgDurationMinutes = kpis.AvgDurationMinutes,

                // session filters state
                FilterFromRaw = from ?? "",
                FilterToRaw = to ?? "",
                FilterSessionId = sessionId,
                FilterAircraftTypeId = aircraftTypeId,
                FilterModeId = modeId,
                FilterOutcome = outcome ?? "",

                // sorting
                SortBy = sortBy,
                SortDir = sortDir,

                HasDockingSessions = sessions.Any(),
                RecentSessions = sessions.Select(s => new DockingSessionRowVM
                {
                    SessionId = s.SessionId,
                    AircraftType = s.AircraftName,
                    Mode = s.ModeName,
                    StartTimeLocal = s.StartTimeLocal,
                    EndTimeLocal = s.EndTimeLocal,
                    DurationMinutes = s.DurationMinutes,
                    IsSuccess = s.IsSuccess,
                    IncidentCount = s.IncidentCount
                }).ToList(),

                SuccessByAircraft = aircraftOrder.Select(name =>
                    new AircraftSuccessVM(name, aircraftMap.TryGetValue(name, out var rate) ? rate : 0.0)
                ).ToList(),

                ManualOverrideSuccess = byMode.FirstOrDefault(x => x.ModeName == "Manual Override")?.SuccessRate ?? 0.0,
                AvdgsAssistedSuccess = byMode.FirstOrDefault(x => x.ModeName == "A-VDGS Assisted")?.SuccessRate ?? 0.0,

                // ✅ incident filters state
                IncFilterFromRaw = incFrom ?? "",
                IncFilterToRaw = incTo ?? "",
                IncFilterSessionId = incSessionId,
                IncFilterSeverity = incSeverity ?? "",
                IncFilterQuery = incQ ?? "",

                RecentIncidents = incidents.Select(i => new IncidentVM
                {
                    SessionId = i.SessionId,
                    Severity = i.Severity,
                    Title = i.Title,
                    Description = i.Description,
                    Date = i.CreatedAt
                }).ToList(),

                AircraftFilterOptions = BuildAircraftOptions(aircraftLookups, aircraftTypeId),
                ModeFilterOptions = BuildModeOptions(modeLookups, modeId),
            };

            return View(vm);
        }

        // ============================
        // MANUAL SESSION ENTRY
        // ============================
        [HttpGet]
        public async Task<IActionResult> Entry()
        {
            ViewData["Title"] = "Operations";
            ViewData["ActiveTab"] = "Operations";

            var vm = new OperationsEntryVM();

            await FillDropdownsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entry(OperationsEntryVM vm)
        {
            ViewData["Title"] = "Operations";
            ViewData["ActiveTab"] = "Operations";

            if (vm.EndTimeLocal <= vm.StartTimeLocal)
                ModelState.AddModelError(nameof(vm.EndTimeLocal), "End time must be greater than start time.");

            if (vm.HasIncident)
            {
                if (string.IsNullOrWhiteSpace(vm.IncidentTitle))
                    ModelState.AddModelError(nameof(vm.IncidentTitle), "Incident title is required.");
                if (string.IsNullOrWhiteSpace(vm.IncidentDescription))
                    ModelState.AddModelError(nameof(vm.IncidentDescription), "Incident description is required.");
                if (string.IsNullOrWhiteSpace(vm.Severity))
                    ModelState.AddModelError(nameof(vm.Severity), "Severity is required.");
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownsAsync(vm);
                return View(vm);
            }

            int? createdByUserId = null;

            var sessionId = await _ops.CreateSessionAsync(new SessionCreateRequest(
                AircraftTypeId: vm.AircraftTypeId,
                ModeId: vm.ModeId,
                StartTimeLocal: vm.StartTimeLocal,
                EndTimeLocal: vm.EndTimeLocal,
                IsSuccess: vm.IsSuccess,
                CenterlineOffsetM: null,
                DistanceToStopM: null,
                LateralAdjustM: null,
                Notes: vm.Notes,
                CreatedByUserId: createdByUserId
            ));

            if (vm.HasIncident)
            {
                await _ops.AddIncidentAsync(new IncidentCreateRequest(
                    SessionId: sessionId,
                    Severity: vm.Severity,
                    Title: vm.IncidentTitle!.Trim(),
                    Description: vm.IncidentDescription!.Trim(),
                    CreatedByUserId: createdByUserId
                ));
            }

            TempData["OpsMsg"] = $"Session #{sessionId} saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // ADD INCIDENT
        // ============================
        [HttpGet]
        public IActionResult AddIncident(long id)
        {
            ViewData["Title"] = "Operations";
            ViewData["ActiveTab"] = "Operations";
            return View(new AddIncidentVM { SessionId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIncident(AddIncidentVM vm)
        {
            ViewData["Title"] = "Operations";
            ViewData["ActiveTab"] = "Operations";

            if (!ModelState.IsValid)
                return View(vm);

            int? createdByUserId = null;

            await _ops.AddIncidentAsync(new IncidentCreateRequest(
                SessionId: vm.SessionId,
                Severity: vm.Severity,
                Title: vm.Title.Trim(),
                Description: vm.Description.Trim(),
                CreatedByUserId: createdByUserId
            ));

            TempData["OpsMsg"] = $"Incident added to Session #{vm.SessionId}.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Docs()
        {
            ViewData["Title"] = "Operations";
            ViewData["ActiveTab"] = "Operations";
            return View();
        }

        // ============================
        // HELPERS
        // ============================
        private async Task FillDropdownsAsync(OperationsEntryVM vm)
        {
            var aircraft = await _ops.ListAircraftTypesAsync();
            var modes = await _ops.ListOperationModesAsync();

            vm.AircraftOptions = aircraft.Select(a => new SelectListItem(a.Name, a.Id.ToString())).ToList();
            vm.ModeOptions = modes.Select(m => new SelectListItem(m.Name, m.Id.ToString())).ToList();
        }

        private static List<SelectListItem> BuildAircraftOptions(List<LookupDto> items, int? selectedId)
        {
            var list = new List<SelectListItem> { new SelectListItem("All Aircraft", "") };

            foreach (var x in items.OrderBy(x => x.Name))
            {
                list.Add(new SelectListItem(x.Name, x.Id.ToString())
                {
                    Selected = selectedId.HasValue && selectedId.Value == x.Id
                });
            }
            return list;
        }

        private static List<SelectListItem> BuildModeOptions(List<LookupDto> items, int? selectedId)
        {
            var list = new List<SelectListItem> { new SelectListItem("All Modes", "") };

            foreach (var x in items.OrderBy(x => x.Name))
            {
                list.Add(new SelectListItem(x.Name, x.Id.ToString())
                {
                    Selected = selectedId.HasValue && selectedId.Value == x.Id
                });
            }
            return list;
        }

        // Accepts: dd/MM/yyyy OR yyyy-MM-dd
        private static DateTime? ParseDateFlexible(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            s = s.Trim();

            if (DateTime.TryParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var d1))
                return d1;

            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var d2))
                return d2;

            if (DateTime.TryParse(s, out var d3))
                return d3;

            return null;
        }
    }
}