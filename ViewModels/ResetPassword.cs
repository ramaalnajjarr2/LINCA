using System.ComponentModel.DataAnnotations;

namespace LINCA_v1.ViewModels
{
    public class ResetPassword
    {


        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters!")]
        [DataType(DataType.Password)]

        [Display(Name = "NewPassword")]

        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm Password is required!")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New Password and Confirm Password do not match!")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
