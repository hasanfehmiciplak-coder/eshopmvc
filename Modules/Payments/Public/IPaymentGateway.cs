namespace EShopMVC.Modules.Payments.Public
{
    public interface IPaymentGateway
    {
        Task<bool> ChargeAsync(int orderId, decimal amount);
    }
}