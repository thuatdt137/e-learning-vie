namespace e_learning_vie.DTOs.User
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }
        public int? ParentId { get; set; }
        public bool? IsActive { get; set; }
    }
}
