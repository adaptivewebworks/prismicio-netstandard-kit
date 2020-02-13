using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace prismic
{
    public class HttpContextAwarePrismicApiAccessor : DefaultPrismicApiAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextAwarePrismicApiAccessor(PrismicHttpClient prismicHttpClient, ILogger<Api> logger, ICache cache, IHttpContextAccessor httpContextAccessor) : base(prismicHttpClient, logger, cache)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpContextAwarePrismicApiAccessor(PrismicHttpClient prismicHttpClient, ILogger<Api> logger, ICache cache, IOptions<PrismicSettings> settings, IHttpContextAccessor httpContextAccessor) : base(prismicHttpClient, logger, cache, settings)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Api GetApi(ApiData apiData)
            => new Api(apiData,  _cache, _logger, _prismicHttpClient, _httpContextAccessor);
    }
}

