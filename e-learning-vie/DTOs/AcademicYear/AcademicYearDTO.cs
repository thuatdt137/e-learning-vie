namespace e_learning_vie.DTOs.AcademicYear
{
    public class AcademicYearDTO
    {
        public int AcademicYearId { get; set; }

        public string YearName { get; set; } = null!;

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

    }
}
