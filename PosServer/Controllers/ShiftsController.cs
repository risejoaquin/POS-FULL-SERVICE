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
    [Authorize]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpPost]
        public async Task<IActionResult> SyncShift([FromBody] CashRegisterShift shift)
        {
            if (shift == null) return BadRequest();

            var (isSuccess, message, resultShift) = await _shiftService.SyncShiftAsync(shift);
            if (isSuccess)
            {
                return Ok(resultShift);
            }
            else
            {
                if (message.StartsWith("Conflicto"))
                    return Conflict(new { Message = message, ServerVersion = resultShift });
                return BadRequest(new { Message = message });
            }
        }
    }
}
