using System.ComponentModel.DataAnnotations;

namespace AVDGS.Web.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; } = true;

        // Used for redirecting back to the requested page after login
        public string? ReturnUrl { get; set; }

        // For display-only errors like "invalid login"
        public string? GeneralError { get; set; }
    }
}
