namespace PacePalAPI.Requests
{
    public class CommentDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
