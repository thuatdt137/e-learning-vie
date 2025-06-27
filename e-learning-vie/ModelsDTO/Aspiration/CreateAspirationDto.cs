using e_learning_vie.Enums;

namespace e_learning_vie.ModelsDTO.Aspiration
{
    public class StudentAspirationDto
    {
        public List<AspirationItemDto> Aspirations { get; set; }
        public List<SchoolDto> AvailableSchools { get; set; }
    }

    public class AspirationItemDto
    {
        public int AspirationId { get; set; }
        public int? Priority { get; set; }
        public SchoolDto TargetSchool { get; set; }
        public string AcademicYear { get; set; }
    }

    public class SchoolDto
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; }
        public SchoolType SchoolType { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class CreateAspirationDto
    {
        public int TargetSchoolId { get; set; }
        public int AcademicYearId { get; set; }
        public int Priority { get; set; }
    }
}
