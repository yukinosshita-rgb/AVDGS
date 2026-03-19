using Microsoft.AspNetCore.Mvc;
using AVDGS.Web.Models.ViewModels;

namespace AVDGS.Web.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Settings";
            ViewData["ActiveTab"] = "Settings";

            var vm = SettingsVM.BuildSample();
            return View(vm);
        }

        // DB-ready POST endpoint (optional now). Keep it for future wiring.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(SettingsVM model)
        {
            ViewData["Title"] = "Settings";
            ViewData["ActiveTab"] = "Settings";

            // TODO: Persist to DB later (EF/SQL Server)
            TempData["Toast"] = "Settings saved (prototype).";
            return RedirectToAction(nameof(Index));
        }
    }
}
