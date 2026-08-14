using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosDomain.Entities;
using PosApplication.Interfaces.Server;
using PosInfrastructure.Services.Server;
using PosDomain.Interfaces;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username)) return BadRequest("Invalid user payload.");

            var (isSuccess, message, resultUser) = await _userService.CreateOrUpdateUserAsync(user);
            if (isSuccess)
            {
                return Ok(resultUser);
            }
            else
            {
                if (message.StartsWith("Conflicto"))
                    return Conflict(new { Message = message, ServerVersion = resultUser });
                return BadRequest(message);
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            await _userService.DeleteUserAsync(username);
            return Ok();
        }
    }
}
