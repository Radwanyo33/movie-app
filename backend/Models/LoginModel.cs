using System.ComponentModel.DataAnnotations;

namespace Live_Movies.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage ="Password must be atleast 6 character long")]
        public string Password { get; set; } = string.Empty ;
    }
}
