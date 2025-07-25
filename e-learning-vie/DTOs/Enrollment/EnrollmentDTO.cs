namespace e_learning_vie.DTOs.Enrollment
{
    public class EnrollmentDTO
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? Conduct { get; set; }
        public int ClassSessionId { get; set; }
        public DateOnly JoinedDate { get; set; }
    }
}
