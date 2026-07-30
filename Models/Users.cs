using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LINCA_v1.Models
{
    public class Users : IdentityUser
    {
        public bool isSeller { get; set; } = false;
        [Required]

        public string FirstName { get; set; } = "";
        [Required]

        public string LastName { get; set; } = "";

        // Editable
        [Required(ErrorMessage = "University is required")]
        public string University { get; set; } = "";

        [Required(ErrorMessage = "Date of birth is required")]
        [MinAge(17)]
        public DateTime DateOfBirth { get; set; }
    }
}
