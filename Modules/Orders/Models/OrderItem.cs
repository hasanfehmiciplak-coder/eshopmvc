using EShopMVC.Models;
using EShopMVC.Modules.Orders.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Price { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int RefundedQuantity { get; set; }

    // 🔑 Ödeme sonrası dolar
    public string? PaymentTransactionId { get; set; }
}