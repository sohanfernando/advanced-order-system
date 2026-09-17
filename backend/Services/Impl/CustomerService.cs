using AdvancedOrderSystem.Models.DTOs.Customer;
using AdvancedOrderSystem.Models.Entities;
using AdvancedOrderSystem.Repositories;

namespace AdvancedOrderSystem.Services.Impl;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerResponse>> GetAllCustomerAsync()
    {
        var customers = await _customerRepository.GetAllAsync();

        return customers.Select(MapToResponse).ToList();
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
        {
            return null;
        }

        return MapToResponse(customer);
    }

    private static CustomerResponse MapToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt
        };
    }
}