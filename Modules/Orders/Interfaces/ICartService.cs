using EShopMVC.Modules.Orders.Domain.Entities;

public interface ICartService
{
    Task<List<CartItem>> GetItemsAsync();

    Task AddAsync(int productId, int quantity = 1);

    Task UpdateQuantityAsync(int productId, int quantity);

    Task RemoveAsync(int productId);

    Task<int> GetItemCountAsync();

    Task<int> GetCartItemCountAsync();

    Task<List<CartItem>> GetCartItemsAsync();

    Task ClearCartAsync();

    Task ClearAsync(); // 👈 EKLE
}