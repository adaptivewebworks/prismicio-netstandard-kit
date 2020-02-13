using System;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace prismic
{
    public class InMemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string key, long ttl, JToken item)
        {
            var entry = _memoryCache.CreateEntry(key);
            entry.Value = item;
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl);

            _memoryCache.Set(key, entry);
        }

        public JToken Get(string key)
        {
            using (var cacheEntry = _memoryCache.Get(key) as ICacheEntry)
            {
                if (cacheEntry == null)
                    return null;

                return cacheEntry.Value as JToken;
            }
        }

        public Task<T> GetOrSet<T>(string key, Func<Task<T>> factory)
        {
            return _memoryCache.GetOrCreateAsync(key, async (entry) => {
                var val = await factory();
                entry.Value = val;

                return val;
            } );
        }
    }
}
