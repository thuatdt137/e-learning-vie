using e_learning_vie.Models;

namespace e_learning_vie.DTOs.StudentDtos
{
    public partial class StudentImportDto
    {
        public int StudentId { get; set; }

        public string? IdentityCode { get; set; }
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }
    }
}
