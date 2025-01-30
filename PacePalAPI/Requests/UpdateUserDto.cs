using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class UpdateUserDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public int Age {  get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public EGender Gender { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public bool hasDeletedImage { get; set; }
        [Required]
        public string imageData { get; set; }

    }
}
