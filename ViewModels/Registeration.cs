using System.ComponentModel.DataAnnotations;

namespace LINCA_v1.ViewModels
{
    public class Registeration
    {
        [Required(ErrorMessage = "First Name is required!")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required!")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage ="Phone Number is required!")]
        public string phonenum { get; set; }
        [Required(ErrorMessage = "Password is required!")]
        [StringLength(20,MinimumLength =8, ErrorMessage ="Password must be between 8 and 20 characters!")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required!")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match!")]

        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Date of Birth is required!")]

         [MinAge(17)]
        public DateTime DateOfBirth { get; set; }

}
}
