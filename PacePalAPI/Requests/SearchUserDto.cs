using PacePalAPI.Models;
using PacePalAPI.Services.UserService;
using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class SearchUserDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public int CommonFriends {  get; set; }
        [Required]
        public EFriendshipStatus FriendshipStatus { get; set; }
        [Required]
        public string ImageData { get; set; }
    }
}
