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
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // The parent id comes from the token, never from the request body.
        [HttpPost]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<ActionResult<StudentResponseDto>> Create(CreateStudentDto dto)
        {
            var created = await _studentService.CreateAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = created.StudentNo }, created);
        }

        [HttpGet("mine")]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<ActionResult<List<StudentResponseDto>>> GetMine()
        {
            return Ok(await _studentService.GetMineAsync(User.GetUserId()));
        }

        // Parents see only their own students. Staff see any.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StudentResponseDto>> GetById(int id)
        {
            return Ok(await _studentService.GetByIdAsync(id, User.GetUserId(), User.GetRole()));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<ActionResult<StudentResponseDto>> Update(int id, UpdateStudentDto dto)
        {
            return Ok(await _studentService.UpdateAsync(id, User.GetUserId(), dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Parent)]
        public async Task<IActionResult> Delete(int id)
        {
            await _studentService.DeleteAsync(id, User.GetUserId());
            return NoContent();
        }

        // Staff: list and search students by name, e.g. api/Student?search=smith
        [HttpGet]
        [Authorize(Roles = RoleNames.Staff)]
        public async Task<ActionResult<List<StudentResponseDto>>> GetAll([FromQuery] string? search)
        {
            return Ok(await _studentService.GetAllAsync(search));
        }
    }
}