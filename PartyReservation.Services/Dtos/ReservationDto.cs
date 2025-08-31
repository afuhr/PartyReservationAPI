using PartyReservation.Shared.Entities;

namespace PartyReservation.Services.Dtos
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int HallId { get; set; }
        public HallDto? Hall { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string ReservedBy { get; set; } = string.Empty;
    }
}
