using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace prismic
{
    public class DefaultPrismicApiAccessor : IPrismicApiAccessor
    {

        private readonly PrismicHttpClient _prismicHttpClient;
        private readonly ILogger<Api> _logger;
        private readonly ICache _cache;
        private readonly PrismicSettings _settings;

        public DefaultPrismicApiAccessor(PrismicHttpClient prismicHttpClient, ILogger<Api> logger, ICache cache)
        {
            _prismicHttpClient = prismicHttpClient;
            _logger = logger;
            _cache = cache;
        }

        public DefaultPrismicApiAccessor(PrismicHttpClient prismicHttpClient, ILogger<Api> logger, ICache cache, IOptions<PrismicSettings> settings) : this(prismicHttpClient, logger, cache)
        {
            _settings = settings.Value;
            
            if(_settings == null)
                throw new ArgumentNullException(nameof(settings), "Settings must not be null");

            if(string.IsNullOrWhiteSpace(_settings.Endpoint))
                throw new ArgumentException(nameof(_settings.Endpoint), "Invalid endpoint uri");
        }

        /**
         * Entry point to get an {@link Api} object from settings
         */
        public Task<Api> GetApi()
            => GetApi(_settings.Endpoint, _settings.AccessToken);

        /**
		* Entry point to get an {@link Api} object.
		* Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api");</code>
		*
		* @param url the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		* @return the usable API object
		*/
        public Task<Api> GetApi(string endpoint) 
			=> GetApi(endpoint, null);

        /**
		* Entry point to get an {@link Api} object.
		* Example: <code>API api = API.get("https://lesbonneschoses.prismic.io/api", null, new Cache.BuiltInCache(999), new Logger.PrintlnLogger());</code>
		*
		* @param endpoint the endpoint of your prismic.io content repository, typically https://yourrepoid.prismic.io/api
		* @param accessToken Your Oauth access token if you wish to use one (to access future content releases, for instance)
		*/
        public async Task<Api> GetApi(string endpoint, string accessToken)
        {
            var url = endpoint;

            if (!string.IsNullOrWhiteSpace(accessToken))
                url += $"?access_token={WebUtility.UrlEncode(accessToken)}";

            JToken json = _cache.Get(url);

            if (json == null)
            {
                json = await _prismicHttpClient.Fetch(url, _logger, _cache);
                _cache.Set(url, 5000L, json);
            }
            ApiData apiData = ApiData.Parse(json);
            return new Api(apiData, _cache, _logger, _prismicHttpClient);
        }

        
    }
}

