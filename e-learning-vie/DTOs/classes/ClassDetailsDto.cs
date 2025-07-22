using e_learning_vie.Models;

public class ClassDetailsDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = null!;
    public int? SchoolId { get; set; }
    public int? TeacherId { get; set; }

    public ClassDetailsDto() { }

    public ClassDetailsDto(Class cls)
    {
        ClassId = cls.ClassId;
        ClassName = cls.ClassName;
        SchoolId = cls.SchoolId;
    }

    // Cập nhật thông tin Class từ DTO
    public static Class MapToClass(ClassDetailsDto dto, Class cls, Teacher? teacher = null)
    {
        cls.ClassName = dto.ClassName;
        cls.SchoolId = dto.SchoolId;
        return cls;
    }
}
