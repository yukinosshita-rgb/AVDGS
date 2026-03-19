namespace AVDGS.Web.Models.ViewModels
{
    public class DashboardOverviewVM
    {
        public string BadgeText { get; set; } = "";
        public string HeroTitleTop { get; set; } = "";
        public string HeroTitleAccent { get; set; } = "";
        public string HeroSubtitle { get; set; } = "";

        public string CamerasStatus { get; set; } = "Unknown";
        public string SensorsStatus { get; set; } = "Unknown";

        public List<ActionCardVM> QuickActions { get; set; } = new();
        public List<ModuleCardVM> Modules { get; set; } = new();
    }

    public record ActionCardVM(string Title, string Icon, string Kind, string Url);
    public record ModuleCardVM(string Title, string Description, string Url);
}