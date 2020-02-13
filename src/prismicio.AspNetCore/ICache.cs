using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace prismic
{
    public interface ICache {
		void Set(string key, long ttl, JToken item);
		JToken Get (string key);
		Task<T> GetOrSet<T>(string key, Func<Task<T>> factory);
	}
}
