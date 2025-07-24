namespace e_learning_vie.Models
{
    public partial class TeacherSubject
    {
        public int TeaccherSubjectId { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }

        public virtual Teacher Teacher { get; set; }
        public virtual Subject Subject { get; set; }
    }
}
