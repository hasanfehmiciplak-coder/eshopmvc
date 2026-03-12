using EShopMVC.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TimelineController : Controller
    {
        private readonly AppDbContext _context;

        public TimelineController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Detail(string type, int id)
        {
            return type switch
            {
                "Payment" => PartialView("_PaymentDetail",
                    _context.PaymentLogs.Find(id)),

                "Refund" => PartialView("_RefundDetail",
                    _context.PartialRefunds.Find(id)),

                "Fraud" => PartialView("_FraudDetail",
                    _context.FraudFlags.Find(id)),

                _ => Content("Geçersiz kayıt")
            };
        }
    }
}