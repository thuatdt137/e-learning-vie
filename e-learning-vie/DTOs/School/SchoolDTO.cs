namespace e_learning_vie.DTOs.School
{
    public class SchoolDTO
    {
        public int SchoolId { get; set; }

        public string SchoolName { get; set; } = null!;

        public string? SchoolType { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public int? PrincipalId { get; set; }
    }
}
