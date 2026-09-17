using AdvancedOrderSystem.Data;
using AdvancedOrderSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedOrderSystem.Repositories.Impl;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetAllAsync(
        string? status,
        int? customerId,
        int page,
        int pageSize
    )
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o =>
                o.Status == status);
        }

        if (customerId.HasValue)
        {
            query = query.Where(o =>
                o.CustomerId == customerId.Value);
        }

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(
        string? status,
        int? customerId)
    {
        var query = _context.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o =>
                o.Status == status);
        }

        if (customerId.HasValue)
        {
            query = query.Where(o =>
                o.CustomerId == customerId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<bool> TryCancelAsync(int id)
    {
        var affectedRows = await _context.Orders
            .Where(o =>
                o.Id == id &&
                o.Status != OrderStatus.Cancelled)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(o => o.Status, OrderStatus.Cancelled));

        return affectedRows == 1;
    }
}
