using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class UpdateUserDto
    {
        [Required]
        required public string FirstName { get; set; }
        [Required]
        required public string LastName { get; set; }
 
        required public string Bio { get; set; }
        [Required]
        public int Age {  get; set; }
        [Required]
        required public string Country { get; set; }
        [Required]
        required public string City { get; set; }
        [Required]
        public EGender Gender { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public bool hasDeletedImage { get; set; }
        [Required]
        required public string ImageData { get; set; }

    }
}
