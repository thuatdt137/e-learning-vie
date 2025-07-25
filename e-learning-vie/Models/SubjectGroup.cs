namespace e_learning_vie.Models
{
    public class SubjectGroup
    {
        public int SubjectGroupId { get; set; }
        public string SubjectGroupName { get; set; }
        public int LeadTeacherId { get; set; }
        public virtual Teacher LeadTeacher { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    }
}
