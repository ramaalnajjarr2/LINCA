using System.ComponentModel.DataAnnotations;

namespace LINCA_v1.ViewModels
{
    public class VerifyEmail
    {
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
