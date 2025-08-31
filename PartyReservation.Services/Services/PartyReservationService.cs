using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartyReservation.Data;
using PartyReservation.Services.Dtos;
using PartyReservation.Shared.Entities;
using PartyReservation.Shared.Responses;

namespace PartyReservation.Services.Services
{
    public class PartyReservationService
    {
        private readonly IMapper _mapper;
        private readonly TimeOnly _startTime = TimeOnly.Parse("09:00");
        private readonly TimeOnly _endTime = TimeOnly.Parse("18:00");
        private readonly int _cleanTimeInMinutes = 30;
        private readonly int _minReservationTimeInMinutes = 60;

        protected readonly PartyReservationContext _context;

        public PartyReservationService(IMapper mapper, PartyReservationContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReservationDto>> GetReservationsAsync(DateOnly date)
        {
            var reservations = await _context.Reservations
                .Include(x => x.Hall)
                .Where(r => r.Date == date)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReservationDto>>(reservations);
        }

        public async Task<ServiceResponse<ReservationDto>> NewReservationsAsync(ReservationDto dto)
        {
            try
            {
                if (dto == null)
                    return ServiceResponse<ReservationDto>.Fail("La reserva no puede ser nula", ErrorCode.ValidationError);

                if (dto.HallId <= 0)
                    return ServiceResponse<ReservationDto>.Fail("Debe especificar un salón válido", ErrorCode.ValidationError);

                if (dto.Date == default)
                    return ServiceResponse<ReservationDto>.Fail("Debe especificar una fecha", ErrorCode.ValidationError);

                if (dto.Date < DateOnly.FromDateTime(DateTime.Today))
                    return ServiceResponse<ReservationDto>.Fail("La fecha de reserva no puede ser anterior a hoy", ErrorCode.ValidationError);

                if (dto.StartTime < _startTime)
                    return ServiceResponse<ReservationDto>.Fail("La hora de reserva no puede ser anterior a las 09:00 AM", ErrorCode.ValidationError);

                if (dto.EndTime > _endTime)
                    return ServiceResponse<ReservationDto>.Fail("La hora de fin reserva no puede ser posterior a las 06:00 PM", ErrorCode.ValidationError);

                if (!(dto.StartTime <  dto.EndTime))
                    return ServiceResponse<ReservationDto>.Fail("La hora de inicio debe ser anterior a la hora de finalización", ErrorCode.ValidationError);

                if ((dto.EndTime - dto.StartTime).TotalMinutes < _minReservationTimeInMinutes)
                    return ServiceResponse<ReservationDto>.Fail($"La reserva no puede ser menor a {_minReservationTimeInMinutes} minutos", ErrorCode.ValidationError);

                var hallExist = await _context.Halls
                   .Where(x => x.Id == dto.HallId)
                   .FirstOrDefaultAsync();

                if (hallExist == null)
                    return ServiceResponse<ReservationDto>.Fail($"No existe el salón {dto.HallId}", ErrorCode.ValidationError);

                var reservations = (await _context.Reservations
                   .Include(x => x.Hall)
                   .Where(r => r.Date == dto.Date
                          && dto.HallId == r.HallId)
                   .ToListAsync())
                   .Where(r => dto.StartTime < r.EndTime.AddMinutes(_cleanTimeInMinutes)
                           && dto.EndTime > r.StartTime)
                   .ToList();

                if (reservations != null && reservations.Any())
                {
                    var overlapReservation = reservations.First();
                    return  ServiceResponse<ReservationDto>.Fail($"El salón posee una reserva en la franja horaria {overlapReservation.StartTime} a {overlapReservation.EndTime}", ErrorCode.ValidationError);
                }

                var entity = _mapper.Map<Reservation>(dto);

                var result = await _context.Reservations.AddAsync(entity);
                await _context.SaveChangesAsync();

                var savedDto = _mapper.Map<ReservationDto>(result.Entity);

                return new ServiceResponse<ReservationDto>() { Data = savedDto };
            }
            catch (DbUpdateException ex)
            {
                return ServiceResponse<ReservationDto>.Fail("Error en base de datos", ErrorCode.DatabaseError);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ReservationDto>.Fail("Error inesperado", ErrorCode.UnexpectedError);
            }
        }
    }
}
