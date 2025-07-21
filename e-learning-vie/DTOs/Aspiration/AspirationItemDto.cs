using e_learning_vie.DTOs.School;

namespace e_learning_vie.DTOs.Aspiration
{
    public class AspirationItemDto
    {
        public int AspirationId { get; set; }
        public int Priority { get; set; }
        public SchoolDTO TargetSchool { get; set; } = new SchoolDTO();
        public string AcademicYear { get; set; } = string.Empty;
    }
}
