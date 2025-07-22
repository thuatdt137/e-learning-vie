using System.Security.Cryptography.Pkcs;

namespace e_learning_vie.Models
{
    public partial class SubjectGrade
    {
        public int SubjectGradeId { get; set; }
        public int SubjectId { get; set; }
        public int GradeId { get; set; }
        public double Weight { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual Grade Grade { get; set; }

        public ICollection<StudentScore> StudentScores { get; set; } = new List<StudentScore>();
    }
}
