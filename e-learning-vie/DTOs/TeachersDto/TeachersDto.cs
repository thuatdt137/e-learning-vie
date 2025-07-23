using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.DTOs.TeachersDto
{
    public class TeachersDto
    {
        public int TeacherId { get; set; }

        public string IdentityCode { get; set; } = null!;

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(200, ErrorMessage = "Address must be at most 200 characters.")]
        public string? Address { get; set; }

        [RegularExpression(@"^0\d{8,10}$", ErrorMessage = "Phone must start with 0 and contain 9–11 digits.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "School ID is required.")]
        public int? SchoolId { get; set; }
    }

    public class CreateTeacherDto
    {
        [Required(ErrorMessage = "IdentityCode is required.")]
        public string IdentityCode { get; set; } = null!;

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(200, ErrorMessage = "Address must be at most 200 characters.")]
        public string Address { get; set; }

        [RegularExpression(@"^0\d{8,10}$", ErrorMessage = "Phone must start with 0 and contain 9–11 digits.")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "School ID is required.")]
        public int SchoolId { get; set; }
    }

    public class TeacherScheduleDto
    {
        public int? ClassId { get; set; }

        public string? DayOfWeek { get; set; }

        public string? SubjectName { get; set; }

        public int? Slot { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public string? Room { get; set; }

    }

}
