using Microsoft.EntityFrameworkCore;
using PartyReservation.Shared.Entities;

namespace PartyReservation.Data
{
    public class PartyReservationContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite();
        }
    }
}
