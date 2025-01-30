using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
