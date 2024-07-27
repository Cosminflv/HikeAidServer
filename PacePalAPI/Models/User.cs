namespace PacePalAPI.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public UserModel(int id, string name, string password)
        {
            Id = id;
            Name = name;
            Password = password;
        }
    }
}
