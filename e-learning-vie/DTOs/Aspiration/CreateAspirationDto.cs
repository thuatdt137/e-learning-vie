using e_learning_vie.Enums;
using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.DTOs.Aspiration
{
    public class CreateAspirationDto
    {
        [Required(ErrorMessage = "Năm học là bắt buộc")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Mã trường là bắt buộc")]
        public int SchoolId { get; set; }

        [Required(ErrorMessage = "Thứ tự nguyện vọng là bắt buộc")]
        [Range(1, 3, ErrorMessage = "Thứ tự nguyện vọng phải từ 1 đến 3")]
        public int Order { get; set; }
    }
}
