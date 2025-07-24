namespace e_learning_vie.Models
{
    public partial class ClassSession
    {
        public int ClassSessionId { get; set; }
        public int? TeacherId { get; set; }
        public int ClassId { get; set; }
        public int SemesterId { get; set; }

        public virtual Teacher? HomeroomTeacher { get; set; }
        public virtual Class Class { get; set; }
        public virtual Semester Semester { get; set; }

        public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
