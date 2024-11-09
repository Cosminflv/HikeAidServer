using PacePalAPI.Models;
using PacePalAPI.Services.UserService;

namespace PacePalAPI.Requests
{
    public class SearchUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int CommonFriends {  get; set; }
        public EFriendshipStatus FriendshipStatus { get; set; }
        public string ImageData { get; set; }
    }
}
