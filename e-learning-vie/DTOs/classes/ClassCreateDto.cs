using e_learning_vie.Models;
using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.DTOs.classes
{
    public class ClassCreateDto
    {
        [Required(ErrorMessage = "ClassName không được để trống.")]
        [StringLength(20, ErrorMessage = "ClassName có độ dài tối đa 20 ký tự.")]
        public string ClassName { get; set; } = null!;

        public int? AcademicYearId { get; set; }

        public int? TeacherId { get; set; }

        public int? SchoolId { get; set; }

        public Class ToClass()
        {
            return new Class
            {
                ClassName = this.ClassName,
                AcademicYearId = this.AcademicYearId,
                TeacherId = this.TeacherId,
                SchoolId = this.SchoolId
            };
        }
    }
}
