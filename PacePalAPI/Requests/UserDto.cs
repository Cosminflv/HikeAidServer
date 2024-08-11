namespace PacePalAPI.Requests
{
    public class UserDto
    {
        public string Username { get; set; }

        public string FirstName { get; set; }   

        public string LastName { get; set; }

        public string PasswordHash { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string Bio { get; set; }
    }
}
