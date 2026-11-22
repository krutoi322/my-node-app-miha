using System.Linq;
using System.Threading.Tasks;
using Laba24App.Models;
using Laba24App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Laba24App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _userService.GetAllAsync();
            return Ok(new { data = users });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { error = "User not found", status = 404 });
            return Ok(new { data = user });
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { error = "Validation failed", details = errors });
            }

            var existing = await _userService.GetByEmailAsync(user.Email);
            if (existing != null) return Conflict(new { error = "Email already exists", status = 409 });

            await _userService.CreateAsync(user);
            return Ok(new { data = user });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] User user)
        {
            var existing = await _userService.GetByIdAsync(id);
            if (existing == null) return NotFound(new { error = "User not found", status = 404 });

            user.Id = id;
            await _userService.UpdateAsync(id, user);
            return Ok(new { data = user });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _userService.GetByIdAsync(id);
            if (existing == null) return NotFound(new { error = "User not found", status = 404 });

            await _userService.DeleteAsync(id);
            return Ok(new { data = "Deleted" });
        }
    }
}