using System.ComponentModel.DataAnnotations;
namespace e_learning_vie.Enums
{
    public enum ConductLevel
    {
        [Display(Name = "Tốt")]
        Excellent,

        [Display(Name = "Khá")]
        Good,

        [Display(Name = "Trung bình")]
        Average,

        [Display(Name = "Yếu")]
        Weak
    }
}
