using Newtonsoft.Json.Linq;

namespace prismic
{
    public interface ICache {
		void Set(string key, long ttl, JToken item);
		JToken Get (string key);
	}
}
