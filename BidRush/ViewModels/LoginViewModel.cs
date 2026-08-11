using System.ComponentModel.DataAnnotations;

namespace BidRush.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [StringLength(250)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
