using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PartyReservation.Data;
using PartyReservation.Services.Dtos;
using PartyReservation.Services.Services;
using PartyReservation.Services.Mappers;
using AutoMapper;


namespace PartyReservation.Tests.Services
{
    [TestClass]
    public class PartyReservationServiceTests
    {
        private PartyReservationContext GetDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<PartyReservationContext>()
                .UseSqlite(connection)
                .Options;

            var context = new PartyReservationContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private PartyReservationService GetService(PartyReservationContext context)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });

            var mapper = config.CreateMapper();

            return new PartyReservationService(mapper, context);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenDtoIsNull()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.NewReservationsAsync(null);

            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.AreEqual("La reserva no puede ser nula", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenHallIdInvalid()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReservationDto { HallId = 0 };
            var result = await service.NewReservationsAsync(dto);

            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.AreEqual("Debe especificar un salón válido", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenDateInvalid()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReservationDto { HallId = 1, Date = default };
            var result = await service.NewReservationsAsync(dto);

            Assert.AreEqual("Debe especificar una fecha", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenDateInPast()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReservationDto
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
            };
            var result = await service.NewReservationsAsync(dto);

            Assert.AreEqual("La fecha de reserva no puede ser anterior a hoy", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenStartTimeTooEarly()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReservationDto
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.Parse("08:00"),
                EndTime = TimeOnly.Parse("10:00")
            };
            var result = await service.NewReservationsAsync(dto);

            Assert.AreEqual("La hora de reserva no puede ser anterior a las 09:00 AM", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenEndTimeTooLate()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReservationDto
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.Parse("18:00"),
                EndTime = TimeOnly.Parse("19:00")
            };
            var result = await service.NewReservationsAsync(dto);

            Assert.AreEqual("La hora de fin reserva no puede ser posterior a las 06:00 PM", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenDurationTooShort()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReservationDto
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.Parse("10:00"),
                EndTime = TimeOnly.Parse("10:30")
            };
            var result = await service.NewReservationsAsync(dto);

            Assert.AreEqual("La reserva no puede ser menor a 60 minutos", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenHallNotExist()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var dto = new ReservationDto
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.Parse("10:00"),
                EndTime = TimeOnly.Parse("12:00")
            };
            var result = await service.NewReservationsAsync(dto);

            Assert.AreEqual("No existe el salón 1", result.ErrorMessage);
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldReturnError_WhenOverlap()
        {
            var context = GetDbContext();
            var service = GetService(context);

            context.Halls.Add(new Shared.Entities.Hall
            {
                Id = 1,
                Name = "Salon 1",
            });

            context.Reservations.Add(new Shared.Entities.Reservation
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.Parse("12:00"),
                EndTime = TimeOnly.Parse("14:00"),
                ReservedBy = "Existing"
            });
            await context.SaveChangesAsync();

            var dto = new ReservationDto
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.Parse("13:00"),
                EndTime = TimeOnly.Parse("15:00"),
                ReservedBy = "New"
            };

            var result = await service.NewReservationsAsync(dto);

            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsTrue(result.ErrorMessage.Contains("El salón posee una reserva"));
        }

        [TestMethod]
        public async Task NewReservationsAsync_ShouldCreateReservation_WhenValid()
        {
            var context = GetDbContext();
            var service = GetService(context);

            context.Halls.Add(new Shared.Entities.Hall
            {
                Id = 1,
                Name = "Salon 1",
            });

            await context.SaveChangesAsync();

            var dto = new ReservationDto
            {
                HallId = 1,
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.Parse("10:00"),
                EndTime = TimeOnly.Parse("12:00"),
                ReservedBy = "Andrea"
            };

            var result = await service.NewReservationsAsync(dto);

            Assert.IsNull(result.ErrorMessage);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(dto.HallId, result.Data.HallId);
            Assert.AreEqual(dto.ReservedBy, result.Data.ReservedBy);
        }
    }
}
