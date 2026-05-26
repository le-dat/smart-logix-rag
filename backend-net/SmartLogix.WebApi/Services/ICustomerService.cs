using System.Collections.Generic;
using System.Threading.Tasks;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
