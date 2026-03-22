using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class TransactionService
{
    private readonly AppDbContext _context;

    public TransactionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(Func<Task> action)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            await action();

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        });
    }
}