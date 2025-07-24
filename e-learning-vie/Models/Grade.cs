namespace e_learning_vie.Models
{
    public partial class Grade
    {
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public string GradeDescription { get; set;}

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    }
}
