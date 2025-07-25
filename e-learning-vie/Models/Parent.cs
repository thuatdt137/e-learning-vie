namespace e_learning_vie.Models
{
    public partial class Parent
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

        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();

        public virtual User? User { get; set; }

    }
}
