using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Payments.Models;
using Microsoft.EntityFrameworkCore;

public class PaymentCallbackService
{
    private readonly AppDbContext _context;

    public PaymentCallbackService(AppDbContext context)
    {
        _context = context;
    }

    public async Task HandlePaymentSuccess(
        string transactionId,
        int orderId,
        decimal amount)
    {
        var existing = await _context.PaymentTransactions
            .FirstOrDefaultAsync(x => x.GatewayTransactionId == transactionId);

        if (existing != null)
            return;

        var order = await _context.Orders
            .FirstAsync(o => o.Id == orderId);

        if (order.IsPaid)
            return;

        // ✔ DDD doğru kullanım
        order.MarkAsPaid();

        _context.PaymentTransactions.Add(new PaymentTransaction
        {
            OrderId = orderId,
            GatewayTransactionId = transactionId,
            Amount = amount,
            PaidAt = DateTime.UtcNow,
            Processed = true
        });

        await _context.SaveChangesAsync();
    }
}