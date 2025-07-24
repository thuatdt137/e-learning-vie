using System.ComponentModel.DataAnnotations;
using e_learning_vie.Models;

namespace e_learning_vie.DTOs.classes
{
    public class ClassCreateDto
    {
        [Required(ErrorMessage = "ClassName không được để trống.")]
        [StringLength(20, ErrorMessage = "ClassName có độ dài tối đa 20 ký tự.")]
        public string ClassName { get; set; } = null!;

        public Class ToClass()
        {
            return new Class
            {
                ClassName = this.ClassName
            };
        }
    }
}
