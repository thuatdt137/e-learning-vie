namespace e_learning_vie.DTOs.Aspiration
{
    public class AspirationDto
    {
        public int Id { get; set; }

        public int AcademicYearId { get; set; }

        public int SchoolId { get; set; }

        public string SchoolName { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
