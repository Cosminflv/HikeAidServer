using PacePalAPI.Models;

namespace PacePalAPI.Requests
{
    public class SocialPostDto
    {
        public int Id { get; set; } 

        public string Content { get; set; }
        
        public string ImageUrl { get; set; }
    }
}
