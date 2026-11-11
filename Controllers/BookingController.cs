using BookingSystemAPI.DTOs;
using BookingSystemAPI.Helpers;
using BookingSystemAPI.Models;
using BookingSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // The parent id comes from the token, never from the request body.
        [HttpPost]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<ActionResult<BookingDetailsDto>> Create(CreateBookingDto dto)
        {
            var created = await _bookingService.CreateAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = created.BookingId }, created);
        }

        [HttpGet("mine")]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<ActionResult<List<BookingResponseDto>>> GetMine()
        {
            return Ok(await _bookingService.GetMineAsync(User.GetUserId()));
        }

        // Staff: e.g. api/Booking?slotId=3&status=Confirmed&from=2026-12-01&to=2026-12-31
        [HttpGet]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<List<BookingResponseDto>>> GetAll(
            [FromQuery] int? slotId,
            [FromQuery] BookingStatus? status,
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int? studentNo)
        {
            return Ok(await _bookingService.GetAllAsync(slotId, status, from, to, studentNo));
        }

        // Parents see only their own. Staff see any.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingDetailsDto>> GetById(int id)
        {
            return Ok(await _bookingService.GetByIdAsync(id, User.GetUserId(), User.GetRole()));
        }

        // Handy for phone queries: api/Booking/number/BK-7K4M2X
        [HttpGet("number/{bookingNumber}")]
        public async Task<ActionResult<BookingDetailsDto>> GetByNumber(string bookingNumber)
        {
            return Ok(await _bookingService.GetByNumberAsync(bookingNumber, User.GetUserId(), User.GetRole()));
        }

        // Reschedule: moves the booking to a different slot.
        [HttpPut("{id:int}")]
        public async Task<ActionResult<BookingDetailsDto>> Update(int id, UpdateBookingDto dto)
        {
            return Ok(await _bookingService.UpdateAsync(id, dto, User.GetUserId(), User.GetRole()));
        }

        // A POST, not a DELETE, because nothing is deleted. The body (with an optional reason) may be left out.
        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<BookingDetailsDto>> Cancel(int id, CancelBookingDto? dto)
        {
            return Ok(await _bookingService.CancelAsync(id, dto, User.GetUserId(), User.GetRole()));
        }

        // The clerk marks the booking as completed once the child has arrived.
        [HttpPost("{id:int}/complete")]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<BookingDetailsDto>> Complete(int id)
        {
            return Ok(await _bookingService.CompleteAsync(id));
        }

        // Admin only: undoes a "Completed" status that was set by mistake (only before any marks exist).
        [HttpPost("{id:int}/revert-completion")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<BookingDetailsDto>> RevertCompletion(int id)
        {
            return Ok(await _bookingService.RevertCompletionAsync(id, User.GetUserId()));
        }
    }
}