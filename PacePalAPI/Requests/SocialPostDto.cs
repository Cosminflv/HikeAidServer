using PacePalAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class SocialPostDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string ImageUrl { get; set; }
    }
}
