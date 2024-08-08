namespace PacePalAPI.Models
{
    public class FriendshipModel
    {
        public int Id { get; set; }  // Primary key
        public int UserId { get; set; }  // Foreign key
        public int FriendId { get; set; }  // Foreign key
        public DateTime CreatedAt { get; set; }

        public FriendshipModel(int id, int userId, int friendId, DateTime createdAt, UserModel user, UserModel friend)
        {
            Id = id;
            UserId = userId;
            FriendId = friendId;
            CreatedAt = createdAt;
            User = user;
            Friend = friend;
        }

        public FriendshipModel() { }    

        // Navigation properties
        public UserModel User { get; set; }
        public UserModel Friend { get; set; }
    }
}
