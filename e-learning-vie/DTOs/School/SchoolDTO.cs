using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.DTOs.School
{
    public class SchoolDTO
    {
        public int SchoolId { get; set; }

        public string SchoolName { get; set; } = null!;

        public string? SchoolType { get; set; }

        public string? Address { get; set; }
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }

        public int? PrincipalId { get; set; }

        public SchoolDTO(Models.School school)
        {
            SchoolId = school.SchoolId;
            SchoolName = school.SchoolName;
            SchoolType = school.SchoolType;
            Address = school.Address;
            Phone = school.Phone;
            Email = school.Email;
            PrincipalId = school.PrincipalId;
        }
        public SchoolDTO()
        {
        }


        public e_learning_vie.Models.School ToSchool()
        {
            return new Models.School
            {
                SchoolId = this.SchoolId,
                SchoolName = this.SchoolName,
                SchoolType = this.SchoolType,
                Address = this.Address,
                Phone = this.Phone,
                Email = this.Email,
                PrincipalId = this.PrincipalId
            };
        }
    }
}
