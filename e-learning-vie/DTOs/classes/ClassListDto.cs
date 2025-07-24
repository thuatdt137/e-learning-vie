using e_learning_vie.Models;

namespace e_learning_vie.DTOs.classes
{
    public class ClassListDto
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; } = null!;

        public int? SchoolId { get; set; }

        public int? TeacherId { get; set; }

        public ClassListDto() { }

        public ClassListDto(Class newClass)
        {
            ClassId = newClass.ClassId;
            ClassName = newClass.ClassName;
        }
    }
}
