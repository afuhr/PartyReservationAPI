using AutoMapper;
using PartyReservation.Services.Dtos;
using PartyReservation.Shared.Entities;

namespace PartyReservation.Services.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Reservation, ReservationDto>();
            CreateMap<Hall, HallDto>();

            CreateMap<ReservationDto, Reservation>()
                .ForMember(dest => dest.Hall, opt => opt.Ignore());
        }
    }
}
