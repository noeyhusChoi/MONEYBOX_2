using System;
using System.Threading;
using System.Threading.Tasks;

namespace KIOSK.Infrastructure.Cache
{
    public interface IAppCache
    {
        T? Get<T>(string key);
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
        void Set<T>(string key, T value, TimeSpan? absoluteExpire = null);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpire = null, CancellationToken ct = default);
        void Remove(string key);
        Task RemoveAsync(string key, CancellationToken ct = default);
    }
}
