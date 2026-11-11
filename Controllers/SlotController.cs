using BookingSystemAPI.DTOs;
using BookingSystemAPI.Models;
using BookingSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotController : ControllerBase
    {
        private readonly ISlotService _slotService;

        public SlotController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        // Any logged-in user (parents use this to choose a slot).
        [HttpGet("available")]
        [Authorize]
        public async Task<ActionResult<List<SlotResponseDto>>> GetAvailable()
        {
            return Ok(await _slotService.GetAvailableAsync());
        }

        // Staff: all slots, optionally filtered, e.g. api/Slot?from=2026-12-01&to=2026-12-31&status=Open
        [HttpGet]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<List<SlotResponseDto>>> GetAll(
            [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] SlotStatus? status)
        {
            return Ok(await _slotService.GetAllAsync(from, to, status));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<SlotResponseDto>> GetById(int id)
        {
            return Ok(await _slotService.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<SlotResponseDto>> Create(CreateSlotDto dto)
        {
            var created = await _slotService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.SlotId }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<SlotResponseDto>> Update(int id, UpdateSlotDto dto)
        {
            return Ok(await _slotService.UpdateAsync(id, dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<IActionResult> Delete(int id)
        {
            await _slotService.DeleteAsync(id);
            return NoContent();
        }
    }
}