using e_learning_vie.Models;

namespace e_learning_vie.DTOs.classes
{
    public class ClassDetailsDto
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; } = null!;

        public int? AcademicYearId { get; set; }

        public int? TeacherId { get; set; }

        public int? SchoolId { get; set; }

        public ClassDetailsDto() { }

        public ClassDetailsDto(Class cls)
        {
            this.ClassId = cls.ClassId;
            this.ClassName = cls.ClassName;
            this.AcademicYearId = cls.AcademicYearId;
            this.TeacherId = cls.TeacherId;
            this.SchoolId = cls.SchoolId;
        }

        public static Class map2Class(ClassDetailsDto dto, Class cls)
        {
            cls.ClassName = dto.ClassName;
            cls.AcademicYearId = dto.AcademicYearId;
            cls.TeacherId = dto.TeacherId;
            cls.SchoolId = dto.SchoolId;
            return cls;
        }
    }
}
