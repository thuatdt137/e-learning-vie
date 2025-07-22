using e_learning_vie.Models;

namespace e_learning_vie.DTOs.StudentDtos
{
    public class StudentDetailsDto
    {
        public int StudentId { get; set; }

        public string IdentityCode { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateOnly? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public int? ClassId { get; set; }

        public int? SchoolId { get; set; }

        public StudentDetailsDto() { }

        public StudentDetailsDto(Models.Student student)
        {
            this.StudentId = student.StudentId;
            this.IdentityCode = student.IdentityCode;
            this.FirstName = student.FirstName;
            this.LastName = student.LastName;
            this.DateOfBirth = student.DateOfBirth;
            this.Address = student.Address;
            this.Phone = student.Phone;
            this.Email = student.Email;
        }

        public static Models.Student map2Student(StudentDetailsDto dto, Models.Student student)
        {
            student.StudentId = dto.StudentId;
            student.IdentityCode = dto.IdentityCode;
            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.DateOfBirth = dto.DateOfBirth;
            student.Address = dto.Address;
            student.Phone = dto.Phone;
            student.Email = dto.Email;
            return student;
        }
    }
}
