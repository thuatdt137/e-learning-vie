namespace e_learning_vie.DTOs.AcademicYear
{
    public class AcademicYearDTO
    {
        public int AcademicYearId { get; set; }

        public string YearName { get; set; } = null!;

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        // Thời gian mở đăng ký nguyện vọng
        public DateOnly? AspirationRegistrationStartDate { get; set; }

        // Thời gian đóng đăng ký nguyện vọng
        public DateOnly? AspirationRegistrationEndDate { get; set; }

        // Thời gian đóng chỉnh sửa nguyện vọng (sau này chỉ xem được)
        public DateOnly? AspirationEditDeadline { get; set; }
    }
}
