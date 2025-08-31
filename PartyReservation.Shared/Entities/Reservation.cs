namespace PartyReservation.Shared.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string ReservedBy { get; set; } = string.Empty;
    }

}
