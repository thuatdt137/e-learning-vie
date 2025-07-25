using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.DTOs.TeachersDto
{
    public class TeachersDto
    {
        public int TeacherId { get; set; }

        public string? IdentityCode { get; set; } = null!;

        public string? FirstName { get; set; } = null!;

        public string? LastName { get; set; } = null!;
        public bool IsMale { get; set; } 

        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }
    }

    public class CreateTeacherDto
    {
        [Required(ErrorMessage = "IdentityCode is required.")]
        public string IdentityCode { get; set; } = null!;

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }
        public bool IsMale { get; set; } // Thêm thuộc tính này

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(200, ErrorMessage = "Address must be at most 200 characters.")]
        public string Address { get; set; }

        [RegularExpression(@"^0\d{8,10}$", ErrorMessage = "Phone must start with 0 and contain 9–11 digits.")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }

    public class TeacherScheduleDto
    {
        public DateTime Date { get; set; }
        public string? DayOfWeek { get; set; }
        public string? SlotName { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? SubjectName { get; set; }
        public string? ClassName { get; set; }
        public string? RoomName { get; set; }
    }
}
