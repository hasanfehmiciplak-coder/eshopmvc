using EShopMVC.Modules.Orders.Domain.Entities;

public interface IFraudRuleService
{
    Task EvaluateAsync(Order order);
}