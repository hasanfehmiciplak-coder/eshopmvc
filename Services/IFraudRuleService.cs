using EShopMVC.Modules.Orders.Models;

public interface IFraudRuleService
{
    Task EvaluateAsync(Order order);
}