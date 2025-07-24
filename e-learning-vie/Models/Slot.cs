namespace e_learning_vie.Models
{
    public class Slot
    {
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
