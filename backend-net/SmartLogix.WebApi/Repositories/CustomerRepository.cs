using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly SmartLogixDbContext _context;

        public CustomerRepository(SmartLogixDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.AsNoTracking().ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return false;
            }
            _context.Customers.Remove(customer);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
