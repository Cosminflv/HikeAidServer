using PacePalAPI.Models.Enums;

namespace PacePalAPI.Models
{
    public class FriendshipModel
    {
        public int Id { get; set; }  // Primary key
        public int RequesterId { get; set; }  // Foreign key
        public int ReceiverId { get; set; }  // Foreign key
        public DateTime CreatedAt { get; set; }
        public EFriendshipState Status { get; set; }

        public FriendshipModel(int id, int requesterId, int receiverId, DateTime createdAt, UserModel user, UserModel friend, EFriendshipState status)
        {
            Id = id;
            RequesterId = requesterId;
            ReceiverId = receiverId;
            CreatedAt = createdAt;
            Requester = user;
            Receiver = friend;
            Status = status;
        }

        public FriendshipModel() { }    

        // Navigation properties
        public UserModel? Requester { get; set; }
        public UserModel? Receiver { get; set; }
    }
}
