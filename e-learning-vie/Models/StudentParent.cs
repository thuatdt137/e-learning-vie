namespace e_learning_vie.Models
{
    public class StudentParent
    {
        public int StudentParentId { get; set; }
        public string RelationalName { get; set; }
        public int StudentId { get; set; }
        public int ParentId { get; set; }

        public virtual Student Student { get; set; }
        public virtual Parent Parent { get; set; }
    }
}
