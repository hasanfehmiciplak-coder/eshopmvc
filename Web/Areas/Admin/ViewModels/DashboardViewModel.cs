using EShopMVC.Areas.Admin.ViewModels;
using EShopMVC.Web.ViewModels;

namespace EShopMVC.ViewModels
{
    public class DashboardViewModel
    {
        // 🔢 TOPLAM
        public int TotalOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        // 📅 BUGÜN
        public int TodayOrderCount { get; set; }

        public decimal TodayRevenue { get; set; }

        // ⏳ BEKLEYEN
        public int PendingOrderCount { get; set; }

        // ❌ İPTAL TALEBİ
        public int CancelRequestCount { get; set; }

        // 🧾 SON SİPARİŞLER
        public List<AdminLastOrderViewModel> LastOrders { get; set; } = new();

        // 📊 SİPARİŞ DURUM İSTATİSTİĞİ ✅ (EKSİKTİ)
        public List<OrderStatusStatViewModel> OrderStatusStats { get; set; } = new();

        // 🔥 EN ÇOK SATANLAR
        public List<TopProductViewModel> TopProducts { get; set; } = new();
    }
}