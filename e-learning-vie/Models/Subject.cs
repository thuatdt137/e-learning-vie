using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Subject
{
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = null!;

    public bool IsMainSubject { get; set; }

    public int SubjectPeriod { get; set; }

    public int GradeId { get; set; }

    public int SubjectGroupId { get; set; }


    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();

    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public virtual ICollection<SubjectScore> SubjectScores { get; set; } = new List<SubjectScore>();


    public virtual Grade Grade { get; set; }
    public virtual SubjectGroup SubjectGroup { get; set; }
}
