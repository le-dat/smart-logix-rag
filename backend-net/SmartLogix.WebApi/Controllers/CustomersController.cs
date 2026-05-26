using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartLogix.WebApi.Models;
using SmartLogix.WebApi.Services;

namespace SmartLogix.WebApi.Controllers
{
    [ApiController]
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
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }
            return Ok(customer);
        }

        // POST: api/customers
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCustomer = await _customerService.CreateCustomerAsync(customer);
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest(new { message = "ID mismatch in request." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _customerService.UpdateCustomerAsync(customer);
            if (!updated)
            {
                return NotFound(new { message = $"Customer with ID {id} not found or no changes made." });
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
