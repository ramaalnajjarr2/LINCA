using System.ComponentModel.DataAnnotations;

namespace LINCA_v1.ViewModels
{
    public class ProfileSettings
    {
        // Read-only (from registration) — will be displayed only
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";

        // Editable
        [Required(ErrorMessage = "Phone is required")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "University is required")]
        public string University { get; set; } = "";

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }
    }
}
