using PacePalAPI.Models;

namespace PacePalAPI.Requests
{
    public class UpdateSocialPostRequest
    {
        public SocialPostModel SocialPost { get; set; }
        public int id { get; set; } 
    }
}
