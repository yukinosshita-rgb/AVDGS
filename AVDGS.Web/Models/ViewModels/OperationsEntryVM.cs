using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AVDGS.Web.Models.ViewModels
{
    public class OperationsEntryVM
    {
        public string PageTitle { get; set; } = "Manual Docking Session Entry";
        public string PageSubtitle { get; set; } = "Encode docking session data for Operations analytics";

        [Required]
        [Display(Name = "Aircraft Type")]
        public int AircraftTypeId { get; set; }

        [Required]
        [Display(Name = "Operation Mode")]
        public int ModeId { get; set; }

        [Required]
        public DateTime StartTimeLocal { get; set; } = DateTime.Now;

        [Required]
        public DateTime EndTimeLocal { get; set; } = DateTime.Now.AddMinutes(5);

        [Display(Name = "Result")]
        public bool IsSuccess { get; set; } = true;

        // Optional notes
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Optional incident inline
        public bool HasIncident { get; set; }

        [MaxLength(10)]
        public string Severity { get; set; } = "LOW";

        [MaxLength(120)]
        public string? IncidentTitle { get; set; }

        [MaxLength(600)]
        public string? IncidentDescription { get; set; }

        // Dropdown data
        public List<SelectListItem> AircraftOptions { get; set; } = new();
        public List<SelectListItem> ModeOptions { get; set; } = new();
    }

    public class AddIncidentVM
    {
        public long SessionId { get; set; }

        [Required]
        public string Severity { get; set; } = "LOW";

        [Required, MaxLength(120)]
        public string Title { get; set; } = "";

        [Required, MaxLength(600)]
        public string Description { get; set; } = "";
    }
}