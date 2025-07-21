using System.ComponentModel.DataAnnotations;

namespace e_learning_vie.Enums
{
    public enum SchoolType
    {
        [Display(Name = "Primary School")]
        C1 = 1,

        [Display(Name = "Secondary School")]
        C2 = 2,

        [Display(Name = "High School")]
        C3 = 3
    }
}
