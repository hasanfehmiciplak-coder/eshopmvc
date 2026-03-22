using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Fraud.Repositories
{
    public class FraudFlagRepository : IFraudFlagRepository
    {
        private readonly AppDbContext _context;

        public FraudFlagRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool Exists(int? refundId, FraudReason reason)
        {
            return _context.FraudFlags
                .Any(x =>
                    x.RefundId == refundId &&
                    x.Reason == reason &&
                    !x.IsResolved);
        }

        public void Add(FraudFlag flag)
        {
            _context.FraudFlags.Add(flag);
            _context.SaveChanges();
        }

        public IEnumerable<FraudFlag> GetAllOpen()
        {
            return _context.FraudFlags
                .Where(x => !x.IsResolved)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToList();
        }

        public FraudFlag GetById(int id)
        {
            return _context.FraudFlags
                .FirstOrDefault(x => x.Id == id);
        }

        public void Update(FraudFlag flag)
        {
            _context.FraudFlags.Update(flag);
            _context.SaveChanges();
        }

        public bool Exists(Guid? refundId, FraudReason reason)
        {
            throw new NotImplementedException();
        }
    }
}