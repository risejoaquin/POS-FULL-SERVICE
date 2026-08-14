using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosDomain.Entities;
using PosApplication.Interfaces.Server;
using PosInfrastructure.Services.Server;
using PosDomain.Interfaces;
using System.Threading.Tasks;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class InventoryMovementsController : ControllerBase
    {
        private readonly IInventoryMovementService _inventoryMovementService;

        public InventoryMovementsController(IInventoryMovementService inventoryMovementService)
        {
            _inventoryMovementService = inventoryMovementService;
        }

        [HttpPost]
        public async Task<IActionResult> SyncMovement([FromBody] InventoryMovement movement)
        {
            if (movement == null) return BadRequest();
            var result = await _inventoryMovementService.SyncMovementAsync(movement);
            return Ok(result);
        }
    }
}
