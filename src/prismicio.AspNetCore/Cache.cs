using System;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace prismic
{
    public class InMemoryCache : ICache
	{
		private readonly IMemoryCache _memoryCache;

		public InMemoryCache(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public void Set (string key, long ttl, JToken item) {
			var entry = _memoryCache.CreateEntry(key);
			entry.Value = item;
			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl);
		}

		public JToken Get(string key) => _memoryCache.Get(key) as JToken;
	}
}
