using PacePalAPI.Models;

namespace PacePalAPI.Requests
{
    public class UserDto
    {
        public string Username { get; set; }

        public string FirstName { get; set; }   

        public string LastName { get; set; }

        public string PasswordHash { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public int Weight { get; set; }

        public EGender EGender { get; set; }

        public DateTime Birthdate { get; set; }

        public int CalculateAge()
        {
            DateTime currentDate = DateTime.Now;
            int age = currentDate.Year - Birthdate.Year;

            // Check if the birthday has occurred this year, if not subtract 1 from age
            if (Birthdate.Date > currentDate.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
