namespace EShopMVC.Infrastructure.Caching
{
    public static class CacheKeys
    {
        public static string OrderDetails(int orderId)
            => $"order-details-{orderId}";
    }
}