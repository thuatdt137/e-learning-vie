using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Grade
{
    public int GradeId { get; set; }

    public string? GradeType { get; set; }

    public string? Description { get; set; }

    public ICollection<SubjectGrade> SubjectGrades { get; set;} = new List<SubjectGrade>();
}
