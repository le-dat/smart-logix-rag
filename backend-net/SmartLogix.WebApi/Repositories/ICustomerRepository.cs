using System.Collections.Generic;
using System.Threading.Tasks;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer> CreateAsync(Customer customer);
        Task<bool> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(int id);
    }
}
