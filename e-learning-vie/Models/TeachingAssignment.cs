namespace e_learning_vie.Models
{
    public partial class TeachingAssignment
    {
        public int TeachingAssignmentId { get; set; } 
        public int? TeacherId { get; set; }
        public int? SubjectId { get; set; }
        public int ClassSessionId { get; set; }

        public virtual Teacher? Teacher { get; set; }
        public virtual Subject? Subject { get; set; }
        public virtual ClassSession Session { get; set; } = null!;
    }
}
