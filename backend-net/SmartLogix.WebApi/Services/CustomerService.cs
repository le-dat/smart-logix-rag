using System.Collections.Generic;
using System.Threading.Tasks;
using SmartLogix.WebApi.Models;
using SmartLogix.WebApi.Repositories;

namespace SmartLogix.WebApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            return await _customerRepository.CreateAsync(customer);
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            return await _customerRepository.UpdateAsync(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            return await _customerRepository.DeleteAsync(id);
        }
    }
}
