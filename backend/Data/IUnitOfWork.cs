using Microsoft.EntityFrameworkCore.Storage;

namespace AdvancedOrderSystem.Data;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync();

    Task SaveChangesAsync();
}
