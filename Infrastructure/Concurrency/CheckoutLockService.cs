using System.Collections.Concurrent;

namespace EShopMVC.Infrastructure.Concurrency
{
    public class CheckoutLockService
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks
            = new();

        public async Task<IDisposable> AcquireAsync(string userId)
        {
            var semaphore = _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();

            return new Releaser(userId, semaphore);
        }

        private class Releaser : IDisposable
        {
            private readonly string _userId;
            private readonly SemaphoreSlim _semaphore;

            public Releaser(string userId, SemaphoreSlim semaphore)
            {
                _userId = userId;
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                _semaphore.Release();
            }
        }
    }
}