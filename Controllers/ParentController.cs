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
    public class ParentController : ControllerBase
    {
        private readonly IParentService _parentService;

        public ParentController(IParentService parentService)
        {
            _parentService = parentService;
        }

        // The id comes from the token, so a parent can only ever see their own data.
        [HttpGet("me")]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<ActionResult<ParentDetailsDto>> GetMe()
        {
            return Ok(await _parentService.GetDetailsAsync(User.GetUserId()));
        }

        [HttpPut("me")]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<ActionResult<ParentResponseDto>> UpdateMe(UpdateParentDto dto)
        {
            return Ok(await _parentService.UpdateProfileAsync(User.GetUserId(), dto));
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<List<ParentResponseDto>>> GetAll([FromQuery] string? search)
        {
            return Ok(await _parentService.GetAllAsync(search));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<ParentDetailsDto>> GetById(int id)
        {
            return Ok(await _parentService.GetDetailsAsync(id));
        }
    }
}