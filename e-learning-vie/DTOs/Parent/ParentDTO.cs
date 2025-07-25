namespace e_learning_vie.DTOs.Parent
{
    public class ParentDTO
    {
        public int ParentId { get; set; }
        public string IdentityCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool IsMale { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
