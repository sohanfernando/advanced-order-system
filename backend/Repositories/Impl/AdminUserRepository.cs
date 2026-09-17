// Repositories/Impl/AdminUserRepository.cs
using AdvancedOrderSystem.Data;
using AdvancedOrderSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedOrderSystem.Repositories.Impl;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly ApplicationDbContext _context;

    public AdminUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUser?> GetByIdAsync(int id)
    {
        return await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AdminUser?> GetByEmailAsync(string normalizedEmail)
    {
        return await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
    }

    public async Task<bool> EmailExistsAsync(string normalizedEmail)
    {
        return await _context.AdminUsers
            .AnyAsync(u => u.Email == normalizedEmail);
    }

    public async Task AddAsync(AdminUser user)
    {
        await _context.AdminUsers.AddAsync(user);
    }
}
