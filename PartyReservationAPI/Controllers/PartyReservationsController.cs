using Microsoft.AspNetCore.Mvc;
using PartyReservation.Services.Dtos;
using PartyReservation.Services.Services;

namespace PartyReservation.API.Controllers
{
    [Route("api/reserva")]
    [ApiController]
    public class PartyReservationsController : ControllerBase
    {
        protected readonly PartyReservationService _service;
        public PartyReservationsController(PartyReservationService service)
        {
            _service = service;
        }

        [HttpGet("{date}")]
        public async Task<IActionResult> Get(string date)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest();

            var result = await _service.GetReservationsAsync(parsedDate);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReservationDto request)
        {
            var result = await _service.NewReservationsAsync(request);
            
            if (!result.Success) 
                return BadRequest(new
                {
                    error = result.ErrorMessage,
                    code = result.ErrorCode
                });

            return Ok(result);
        }
    }
}