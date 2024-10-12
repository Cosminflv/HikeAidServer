namespace PacePalAPI.Requests
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Bio { get; set; }

        public string imageData { get; set; }

    }
}
