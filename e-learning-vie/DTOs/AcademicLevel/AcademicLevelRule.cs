using e_learning_vie.Enums;

namespace e_learning_vie.DTOs.AcademicLevel
{
    public class AcademicLevelRule
    {
        public double AverageThreshold { get; set; }
        public double MainSubjectMin { get; set; }
        public double MinScoreRequired { get; set; }
        public ConductLevel MinConduct { get; set; }
    }
}
