using System.ComponentModel.DataAnnotations;
namespace e_learning_vie.Enums
{
    public enum AcademicLevel
    {
        [Display(Name = "Giỏi")]
        Excellent,

        [Display(Name = "Khá")]
        Good,

        [Display(Name = "Trung bình")]
        Average,

        [Display(Name = "Yếu")]
        Weak
    }
}
