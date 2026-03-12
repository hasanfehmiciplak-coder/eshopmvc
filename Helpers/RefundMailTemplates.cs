namespace EShopMVC.Helpers
{
    public static class RefundMailTemplates
    {
        public static string RequestCreated(int orderId, decimal amount)
            => $@"
<h3>İade Talebiniz Alındı</h3>
<p>Sipariş No: <strong>{orderId}</strong></p>
<p>İade Tutarı: <strong>{amount} ₺</strong></p>
<p>Talebiniz finans onayına gönderilmiştir.</p>";

        public static string Approved(int orderId, decimal amount)
            => $@"
<h3>İadeniz Onaylandı</h3>
<p>Sipariş No: <strong>{orderId}</strong></p>
<p><strong>{amount} ₺</strong> tutarındaki iadeniz kartınıza gönderilmiştir.</p>";

        public static string Rejected(int orderId)
            => $@"
<h3>İade Talebiniz Reddedildi</h3>
<p>Sipariş No: <strong>{orderId}</strong></p>
<p>Detaylar için müşteri hizmetleriyle iletişime geçebilirsiniz.</p>";

        public static string Failed(int orderId, string error)
            => $@"
<h3>İade İşlemi Başarısız</h3>
<p>Sipariş No: <strong>{orderId}</strong></p>
<p>Hata: {error}</p>";
    }
}