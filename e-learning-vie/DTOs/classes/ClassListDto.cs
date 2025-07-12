using e_learning_vie.Models;

namespace e_learning_vie.DTOs.classes
{
    public class ClassListDto
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; } = null!;

        public int? AcademicYearId { get; set; }

        public int? TeacherId { get; set; }

        public int? SchoolId { get; set; }

        public ClassListDto() { }

        public ClassListDto(Class newClass)
        {
            ClassId = newClass.ClassId;
            ClassName = newClass.ClassName;
            AcademicYearId = newClass.AcademicYearId;
            TeacherId = newClass.TeacherId;
            SchoolId = newClass.SchoolId;
        }
    }
}
