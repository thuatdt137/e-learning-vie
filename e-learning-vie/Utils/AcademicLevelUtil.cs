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
                return AcademicLevel.Weak.GetDisplayName();

            var levels = new List<(string Name, AcademicLevelRule Rule)>
            {
                (AcademicLevel.Excellent.GetDisplayName(), _rules.Excellent),
                (AcademicLevel.Good.GetDisplayName(), _rules.Good),
                (AcademicLevel.Average.GetDisplayName(), _rules.Average),
                (AcademicLevel.Weak.GetDisplayName(), _rules.Weak)
            };

            foreach(var (name, rule) in levels)
            {
                bool meetsAvg = avgAll >= rule.AverageThreshold;
                bool meetsAllSubjects = subjectAverages.All(score => score >= rule.MinScoreRequired);
                bool meetsMainSubject = true;

                if(name == AcademicLevel.Excellent.GetDisplayName())
                {
                    meetsMainSubject = mainSubjectAverages.Any(avg => avg >= rule.MainSubjectMin);
                }

                if(meetsAvg && meetsAllSubjects && meetsMainSubject)
                {

                    if(conductEnum > rule.MinConduct)
                        continue;

                    return name;
                }
            }

            return AcademicLevel.Weak.GetDisplayName();
        }

    }
}
