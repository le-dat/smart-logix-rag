using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartLogix.WebApi.Models;
using SmartLogix.WebApi.Services;

namespace SmartLogix.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentsController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        // GET: api/shipments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetShipments()
        {
            var shipments = await _shipmentService.GetAllShipmentsAsync();
            return Ok(shipments);
        }

        // GET: api/shipments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Shipment>> GetShipment(int id)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
            {
                return NotFound(new { message = $"Shipment with ID {id} not found." });
            }
            return Ok(shipment);
        }

        // GET: api/shipments/tracking/DMCO-VN-20260001
        [HttpGet("tracking/{trackingNo}")]
        public async Task<ActionResult<Shipment>> GetShipmentByTracking(string trackingNo)
        {
            var shipment = await _shipmentService.GetShipmentByTrackingNoAsync(trackingNo);
            if (shipment == null)
            {
                return NotFound(new { message = $"Shipment with tracking number '{trackingNo}' not found." });
            }
            return Ok(shipment);
        }

        // POST: api/shipments
        [HttpPost]
        public async Task<ActionResult<Shipment>> CreateShipment([FromBody] Shipment shipment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdShipment = await _shipmentService.CreateShipmentAsync(shipment);
                return CreatedAtAction(nameof(GetShipment), new { id = createdShipment.Id }, createdShipment);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // PUT: api/shipments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShipment(int id, [FromBody] Shipment shipment)
        {
            if (id != shipment.Id)
            {
                return BadRequest(new { message = "ID mismatch in request." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _shipmentService.UpdateShipmentAsync(shipment);
            if (!updated)
            {
                return NotFound(new { message = $"Shipment with ID {id} not found or no changes made." });
            }

            return NoContent();
        }

        // DELETE: api/shipments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShipment(int id)
        {
            var deleted = await _shipmentService.DeleteShipmentAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Shipment with ID {id} not found." });
            }

            return NoContent();
        }
    }
}
