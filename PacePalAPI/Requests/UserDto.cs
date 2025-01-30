using PacePalAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public EGender EGender { get; set; }
        [Required]
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
