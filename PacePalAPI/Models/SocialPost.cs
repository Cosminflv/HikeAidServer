namespace PacePalAPI.Models
{
    public class SocialPostModel
    {
        public int Id { get; set; }  // Primary key
        public int UserId { get; set; }  // Foreign key
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; }

        public SocialPostModel(int Id, int UserId,  string Content, DateTime CreatedAt, string ImageUrl)
        {
            this.Id = Id;
            this.UserId = UserId;
            this.Content = Content;
            this.CreatedAt = CreatedAt;
            this.ImageUrl = ImageUrl;
        }

        public SocialPostModel() { }

        // Navigation properties
        public UserModel User { get; set; }
        public List<CommentModel> Comments { get; set; }
        public List<LikeModel> Likes { get; set; }
    }
}
