using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Services
{
    public class CacheService : ICacheService
    {
        #region Fields

        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;

        private static CancellationTokenSource _resetCacheToken = new();

        protected readonly ConcurrentDictionary<string, SemaphoreSlim> CacheEntries = new();

        #endregion

        #region Ctor

        public CacheService(IMemoryCache cache, IOptions<CacheSettings> cacheSettings)
        {
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }

        #endregion

        #region Methods

        public virtual T Get<T>(string key, Func<T> acquire)
        {
            return Get(key, acquire, _cacheSettings.DefaultExpirationInMinutes);
        }

        public virtual T Get<T>(string key, Func<T> acquire, int cacheTime)
        {
            if (_cache.TryGetValue(key, out T cacheEntry)) return cacheEntry;
            var semaphore = CacheEntries.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            semaphore.Wait();
            try
            {
                if (!_cache.TryGetValue(key, out cacheEntry))
                {
                    cacheEntry = acquire();
                    _cache.Set(key, cacheEntry, GetMemoryCacheEntryOptions(cacheTime));
                }
            }
            finally
            {
                semaphore.Release();
            }

            return cacheEntry;
        }

        public virtual Task<T> GetAsync<T>(string key, Func<Task<T>> acquire)
        {
            return GetAsync(key, acquire, _cacheSettings.DefaultExpirationInMinutes);
        }

        public virtual async Task<T> GetAsync<T>(string key, Func<Task<T>> acquire, int cacheTime)
        {
            if (_cache.TryGetValue(key, out T cacheEntry)) return cacheEntry;
            var semaphore = CacheEntries.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(key, out cacheEntry))
                {
                    cacheEntry = await acquire();
                    _cache.Set(key, cacheEntry, GetMemoryCacheEntryOptions(cacheTime));
                }
            }
            finally
            {
                semaphore.Release();
            }

            return cacheEntry;
        }

        public virtual Task<T> SetAsync<T>(string key, Func<Task<T>> acquire)
        {
            return SetAsync(key, acquire, _cacheSettings.DefaultExpirationInMinutes);
        }

        public virtual async Task<T> SetAsync<T>(string key, Func<Task<T>> acquire, int cacheTime)
        {
            var semaphore = CacheEntries.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                var cacheEntry = await acquire();
                _cache.Set(key, cacheEntry, GetMemoryCacheEntryOptions(cacheTime));
                return cacheEntry;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public virtual Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public virtual Task RemoveByPrefix(string prefix)
        {
            var entriesToRemove = CacheEntries.Where(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            foreach (var cacheEntries in entriesToRemove) _cache.Remove(cacheEntries.Key);

            return Task.CompletedTask;
        }

        public virtual Task Clear(bool publisher = true)
        {
            //clear keys
            foreach (var cacheEntry in CacheEntries.Keys.ToList())
                _cache.Remove(cacheEntry);

            //cancel
            _resetCacheToken.Cancel();
            //dispose
            _resetCacheToken.Dispose();

            _resetCacheToken = new CancellationTokenSource();

            return Task.CompletedTask;
        }

        #endregion

        #region Utilities

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions(int cacheTime)
        {
            var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime) }
                .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token))
                .RegisterPostEvictionCallback(PostEvictionCallback);

            return options;
        }

        private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            if (reason != EvictionReason.Replaced)
                CacheEntries.TryRemove(key.ToString(), out var _);
        }

        #endregion
    }
}
