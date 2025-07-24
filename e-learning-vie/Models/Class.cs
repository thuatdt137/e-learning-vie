using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = null!;

    public int? GradeId { get; set; }

    public virtual ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();



    public virtual Grade Grade { get; set; }

}
