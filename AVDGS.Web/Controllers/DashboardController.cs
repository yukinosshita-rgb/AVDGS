using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AVDGS.Web.Models.ViewModels;

namespace AVDGS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Overview";
            ViewData["ActiveTab"] = "Overview";

            var vm = new DashboardOverviewVM
            {
                BadgeText = "System Gallery & Wireframes",
                HeroTitleTop = "Advanced Visual Docking",
                HeroTitleAccent = "Guidance System",
                HeroSubtitle = "Next-generation AI-powered aircraft docking guidance for flight training and operations",

                CamerasStatus = "Online",
                SensorsStatus = "Active",

                QuickActions = new()
                {
                    new ActionCardVM("Start Docking Session", "bi-play-circle", "btn", "/Docking/Live"),
                    new ActionCardVM("View Live Camera", "bi-camera", "btn", "/Docking/Live"),
                    new ActionCardVM("Training Logs", "bi-journal-text", "btn", "/Operations/Index"),

                    // ✅ REPLACED: Admin Panel (Roles) -> Reports
                    new ActionCardVM("View Reports", "bi-bar-chart-line", "btn", "/Reports/Index"),
                },

                Modules = new()
                {
                    new ModuleCardVM("Live Docking", "Real-time guidance dashboard with camera feeds and visual indicators", "/Docking/Live"),
                    new ModuleCardVM("Operations", "Flight school dashboard with logs and analytics", "/Operations/Index"),

                    // ✅ REPLACED: Role Dashboards -> Reports
                    new ModuleCardVM("Reports", "Analytics and performance metrics from docking sessions", "/Reports/Index"),

                    new ModuleCardVM("Settings", "Configuration and calibration controls", "/Settings/Index"),
                    new ModuleCardVM("Documentation", "Component labels and system documentation", "/Operations/Docs"),
                }
            };

            return View(vm);
        }
    }
}