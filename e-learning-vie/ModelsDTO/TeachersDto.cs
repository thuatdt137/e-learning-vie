namespace e_learning_vie.ModelsDTO
{
    public class TeachersDto
    {
        public int TeacherId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? SchoolId { get; set; }
    }
}
