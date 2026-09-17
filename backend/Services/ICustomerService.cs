using AdvancedOrderSystem.Models.DTOs.Customer;

namespace AdvancedOrderSystem.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerResponse>> GetAllCustomerAsync();
        Task<CustomerResponse?> GetCustomerByIdAsync(int id);
    }
}
