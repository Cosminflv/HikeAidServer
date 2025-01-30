using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class CommentDto
    {
        [Required]
        public int PostId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
    }
}
