using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class LikeDto
    {
        [Required]
        public int PostId { get; set; }
        [Required]
        public int UserId { get; set; }
    }
}
