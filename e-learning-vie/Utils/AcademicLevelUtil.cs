using e_learning_vie.DTOs.AcademicLevel;
using e_learning_vie.Enums;

namespace e_learning_vie.Utils
{
    public class AcademicLevelUtil
    {
        private readonly AcademicLevelRulesConfig _rules;

        public AcademicLevelUtil(AcademicLevelRulesConfig rules)
        {
            _rules = rules;
        }

        public string EvaluateConductLevel(
               double avgAll,
               List<double> mainSubjectAverages,
               List<double> subjectAverages,
               string conductDisplayName)
        {
            var conductEnum = EnumExtensions.GetConductEnumFromDisplayName(conductDisplayName);
            if(conductEnum == null)
                return ConductLevel.Weak.GetDisplayName();

            var levels = new List<(string Name, AcademicLevelRule Rule)>
            {
                (ConductLevel.Excellent.GetDisplayName(), _rules.Excellent),
                (ConductLevel.Good.GetDisplayName(), _rules.Good),
                (ConductLevel.Average.GetDisplayName(), _rules.Average),
                (ConductLevel.Weak.GetDisplayName(), _rules.Weak)
            };

            foreach(var (name, rule) in levels)
            {
                bool meetsAvg = avgAll >= rule.AverageThreshold;
                bool meetsAllSubjects = subjectAverages.All(score => score >= rule.MinScoreRequired);
                bool meetsMainSubject = true;

                // Chỉ áp dụng điều kiện main subject với học sinh Giỏi
                if(name == ConductLevel.Excellent.GetDisplayName())
                {
                    meetsMainSubject = mainSubjectAverages.Any(avg => avg >= rule.MainSubjectMin);
                }

                if(meetsAvg && meetsAllSubjects && meetsMainSubject)
                {

                    if(conductEnum > rule.MinConduct)
                        continue;

                    return rule.MinConduct.GetDisplayName();
                }
            }

            return ConductLevel.Weak.GetDisplayName();
        }

    }
}
