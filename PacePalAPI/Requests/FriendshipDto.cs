namespace PacePalAPI.Requests
{
    public class FriendshipDto
    {
        public int Id { get; set; }

        public int receiverId { get; set; }

        public int requesterId { get; set; }

        public string requesterName { get; set; }
    }
}
