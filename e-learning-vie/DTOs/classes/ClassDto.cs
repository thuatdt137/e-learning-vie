using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.DTOs.classes
{
    // DTO dùng để hiển thị trong danh sách
    public class ClassListDto
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? GradeName { get; set; }
        public string? HomeroomTeacherName { get; set; } // THÊM: Tên GVCN
    }

    // DTO dùng để hiển thị chi tiết một lớp
    public class ClassDetailDto
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public int? GradeId { get; set; }
        public string? GradeName { get; set; }
        public string? HomeroomTeacherName { get; set; } // Giữ lại
        public int StudentCount { get; set; }
    }

    public class ClassCreateDto
    {
        [Required(ErrorMessage = "Tên lớp không được để trống.")]
        public string ClassName { get; set; } = null!;

        [Required(ErrorMessage = "Khối không được để trống.")]
        public int GradeId { get; set; }

        // THÊM: ID của giáo viên chủ nhiệm
        public int? TeacherId { get; set; }
    }

    public class ClassUpdateDto
    {
        [Required]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Tên lớp không được để trống.")]
        public string ClassName { get; set; } = null!;

        [Required(ErrorMessage = "Khối không được để trống.")]
        public int GradeId { get; set; }

        // THÊM: ID của giáo viên chủ nhiệm
        public int? TeacherId { get; set; }
    }
}