using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartLogix.WebApi.DTOs;
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
        public async Task<ActionResult<IEnumerable<ShipmentDto>>> GetShipments()
        {
            var shipments = await _shipmentService.GetAllShipmentsAsync();
            var shipmentDtos = shipments.Select(s => s.ToDto());
            return Ok(shipmentDtos);
        }

        // GET: api/shipments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShipmentDto>> GetShipment(int id)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
            {
                return NotFound(new { message = $"Shipment with ID {id} not found." });
            }
            return Ok(shipment.ToDto());
        }

        // GET: api/shipments/tracking/DMCO-VN-20260001
        [HttpGet("tracking/{trackingNo}")]
        public async Task<ActionResult<ShipmentDto>> GetShipmentByTracking(string trackingNo)
        {
            var shipment = await _shipmentService.GetShipmentByTrackingNoAsync(trackingNo);
            if (shipment == null)
            {
                return NotFound(new { message = $"Shipment with tracking number '{trackingNo}' not found." });
            }
            return Ok(shipment.ToDto());
        }

        // POST: api/shipments
        [HttpPost]
        public async Task<ActionResult<ShipmentDto>> CreateShipment([FromBody] ShipmentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var shipmentEntity = dto.ToEntity();
                var createdShipment = await _shipmentService.CreateShipmentAsync(shipmentEntity);
                
                // Fetch the fully loaded shipment with Route and Customer populated for UI rendering
                var fullyLoadedShipment = await _shipmentService.GetShipmentByIdAsync(createdShipment.Id);
                var responseDto = fullyLoadedShipment?.ToDto() ?? createdShipment.ToDto();
                
                return CreatedAtAction(nameof(GetShipment), new { id = createdShipment.Id }, responseDto);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // PUT: api/shipments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShipment(int id, [FromBody] ShipmentUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch in request." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingShipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (existingShipment == null)
            {
                return NotFound(new { message = $"Shipment with ID {id} not found." });
            }

            dto.UpdateEntity(existingShipment);
            var updated = await _shipmentService.UpdateShipmentAsync(existingShipment);
            if (!updated)
            {
                return BadRequest(new { message = "Failed to update shipment. No changes were made or a database error occurred." });
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
