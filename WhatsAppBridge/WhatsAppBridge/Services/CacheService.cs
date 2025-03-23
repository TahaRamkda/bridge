using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Services
{
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;

        public CacheService(IMemoryCache cache, IOptions<CacheSettings> cacheSettings)
        {
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
        }

        public T GetOrSet<T>(string key, Func<T> getDataFunction)
        {
            if (!_cache.TryGetValue(key, out T cachedData))
            {
                cachedData = getDataFunction();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationInMinutes));

                _cache.Set(key, cachedData, cacheOptions);
            }

            return cachedData;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
