using PacePalAPI.Models;
using PacePalAPI.Models.Enums;

namespace PacePalAPI.Requests
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Bio { get; set; }

        public int Age {  get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public EGender Gender { get; set; }

        public DateTime BirthDate { get; set; }

        public int Weight { get; set; }

        public bool hasDeletedImage { get; set; }

        public string imageData { get; set; }

    }
}
