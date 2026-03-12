namespace EShopMVC.Helpers
{
    public static class IyzicoErrorMapper
    {
        public static string Map(string errorCode, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
                return errorMessage ?? "Ödeme başarısız";

            return errorCode switch
            {
                "10051" => "Kart limiti yetersiz",
                "10005" => "CVV hatalı",
                "10012" => "Banka işlemi reddetti",
                "10041" => "Kart geçersiz",
                "10043" => "Kart süresi dolmuş",
                "10034" => "Kart sahibinin bilgileri doğrulanamadı",
                "10054" => "3D doğrulama başarısız",
                "10001" => "Ödeme sırasında teknik hata oluştu",

                _ => errorMessage ?? $"Ödeme başarısız (Code: {errorCode})"
            };
        }
    }
}