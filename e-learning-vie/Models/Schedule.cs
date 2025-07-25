using System;
using System.Collections.Generic;

namespace e_learning_vie.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int TeachingAssignmentId { get; set; }

    public DateOnly Date { get; set; }

    public int SlotId { get; set; }

    public int? RoomId { get; set; }

    public virtual Room Room { get; set; }

    public virtual Slot Slot { get; set; }

    public virtual TeachingAssignment TeachingAssignment { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

}
