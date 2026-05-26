using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogix.WebApi.DTOs;
using SmartLogix.WebApi.Models;
using SmartLogix.WebApi.Services;

namespace SmartLogix.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var customerDtos = customers.Select(c => c.ToDto());
            return Ok(customerDtos);
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }
            return Ok(customer.ToDto());
        }

        // POST: api/customers
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerEntity = dto.ToEntity();
            var createdCustomer = await _customerService.CreateCustomerAsync(customerEntity);
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer.ToDto());
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch in request." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCustomer = await _customerService.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            dto.UpdateEntity(existingCustomer);
            var updated = await _customerService.UpdateCustomerAsync(existingCustomer);
            if (!updated)
            {
                return BadRequest(new { message = "Failed to update customer. No changes were made or a database error occurred." });
            }

            return NoContent();
        }

        // DELETE: api/customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var deleted = await _customerService.DeleteCustomerAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            return NoContent();
        }
    }
}
