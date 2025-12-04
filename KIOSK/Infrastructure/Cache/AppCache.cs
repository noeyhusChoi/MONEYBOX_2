using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KIOSK.Infrastructure.Cache
{
    /// <summary>
    /// 데스크톱/키오스크 환경에 맞춘 단순 메모리 캐시(ConcurrentDictionary 기반).
    /// 절대 만료만 지원하며, Dispose가 필요 없습니다.
    /// </summary>
    public sealed class AppCache : IAppCache
    {
        private sealed class Entry
        {
            public object? Value { get; init; }
            public DateTimeOffset? ExpireAt { get; init; }
        }

        private readonly ConcurrentDictionary<string, Entry> _entries = new();

        public T? Get<T>(string key)
        {
            if (!_entries.TryGetValue(key, out var entry))
                return default;

            if (entry.ExpireAt is { } exp && exp <= DateTimeOffset.UtcNow)
            {
                _entries.TryRemove(key, out _);
                return default;
            }

            return entry.Value is T val ? val : default;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
            => Task.FromResult(Get<T>(key));

        public void Set<T>(string key, T value, TimeSpan? absoluteExpire = null)
        {
            var expireAt = absoluteExpire.HasValue
                ? DateTimeOffset.UtcNow.Add(absoluteExpire.Value)
                : null as DateTimeOffset?;

            _entries[key] = new Entry { Value = value, ExpireAt = expireAt };
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpire = null, CancellationToken ct = default)
        {
            Set(key, value, absoluteExpire);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _entries.TryRemove(key, out _);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}
