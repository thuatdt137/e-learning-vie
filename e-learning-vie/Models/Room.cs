namespace e_learning_vie.Models
{
    public partial class Room
    {
        public int RoomId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    }
}
