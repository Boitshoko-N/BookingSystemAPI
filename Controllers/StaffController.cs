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
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<List<StaffResponseDto>>> GetAll()
        {
            return Ok(await _staffService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<StaffResponseDto>> GetById(int id)
        {
            return Ok(await _staffService.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<StaffResponseDto>> Create(CreateStaffDto dto)
        {
            var created = await _staffService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.StaffNo }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<StaffResponseDto>> Update(int id, UpdateStaffDto dto)
        {
            return Ok(await _staffService.UpdateAsync(id, dto));
        }

        // Any staff member can change their own password.
        [HttpPost("me/change-password")]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            await _staffService.ChangePasswordAsync(User.GetUserId(), dto);
            return NoContent();
        }
    }
}