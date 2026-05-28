using System.ComponentModel.DataAnnotations;

namespace login.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage= "Emial is required")]
        [EmailAddress]

        public string Email { get; set; }


        [Required(ErrorMessage = "Password required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remeber me ?")]

        public bool RememberMe { get; set; }
    }
}
