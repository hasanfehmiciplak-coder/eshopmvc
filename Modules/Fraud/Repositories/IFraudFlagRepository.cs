using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Repositories;

namespace EShopMVC.Modules.Fraud.Repositories
{
    public interface IFraudFlagRepository
    {
        // 1️⃣ Aynı flag var mı? (idempotency)
        bool Exists(Guid? refundId, FraudReason reason);

        // 2️⃣ Yeni fraud flag ekle
        void Add(FraudFlag flag);

        // 3️⃣ Admin liste ekranı için
        IEnumerable<FraudFlag> GetAllOpen();

        // 4️⃣ Resolve için tekil kayıt
        FraudFlag GetById(int id);

        // 5️⃣ Resolve sonrası güncelle
        void Update(FraudFlag flag);
    }
}