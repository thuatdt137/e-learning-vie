namespace e_learning_vie.DTOs.StudentDtos
{
    // DTO cho danh sách
    public class StudentListDto
    {
        public int StudentId { get; set; }
        public string? IdentityCode { get; set; }
        public string? FullName { get; set; }
        public bool IsMale { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? CurrentClassName { get; set; }
    }

    // DTO cho thông tin chi tiết
    public class StudentDetailsDto
    {
        public int StudentId { get; set; }
        public string? IdentityCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsMale { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CurrentClassName { get; set; }
        public string? HomeroomTeacherName { get; set; }
        public List<ParentInfoDto> Parents { get; set; } = new List<ParentInfoDto>();
    }

    public class ParentInfoDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
    }

    // DTO để tạo mới
    public class StudentCreateDto
    {
        public string IdentityCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool IsMale { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    // DTO để cập nhật
    public class StudentUpdateDto
    {
        public int StudentId { get; set; }
        public string? IdentityCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsMale { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}