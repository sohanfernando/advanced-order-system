// Repositories/IAdminUserRepository.cs
using AdvancedOrderSystem.Models.Entities;

namespace AdvancedOrderSystem.Repositories;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByIdAsync(int id);

    Task<AdminUser?> GetByEmailAsync(string normalizedEmail);

    Task<bool> EmailExistsAsync(string normalizedEmail);

    Task AddAsync(AdminUser user);
}
