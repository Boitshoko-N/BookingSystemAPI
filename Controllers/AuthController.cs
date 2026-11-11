using BookingSystemAPI.DTOs;
using BookingSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("parent/register")]
        [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ParentResponseDto>> RegisterParent(RegisterParentDto dto)
        {
            var parent = await _authService.RegisterParentAsync(dto);
            return StatusCode(StatusCodes.Status201Created, parent);
        }

        [HttpPost("parent/login")]
        public async Task<ActionResult<LoginResponseDto>> ParentLogin(LoginDto dto)
        {
            return Ok(await _authService.LoginParentAsync(dto));
        }

        [HttpPost("staff/login")]
        public async Task<ActionResult<LoginResponseDto>> StaffLogin(LoginDto dto)
        {
            return Ok(await _authService.LoginStaffAsync(dto));
        }
    }
}