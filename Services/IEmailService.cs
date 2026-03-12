using EShopMVC.Areas.Admin.ViewModels;

namespace EShopMVC.Services
{
    public interface IEmailService
    {
        Task SendOrderSuccessMailAsync(OrderSuccessMailVM model);

        Task SendPaymentFailedMailAsync(PaymentFailedMailVM model);
    }
}