using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace prismic.AspNetCore.Tests
{
    public static class TestHelper
    {
        public static readonly string Endpoint = "https://apsnet-core-sdk.cdn.prismic.io/api";
        public static DefaultPrismicApiAccessor GetAccessor(PrismicSettings settings = null)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddLogging()
                    .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<prismic.Api>();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var inMemoryCache = new InMemoryCache(memoryCache);

            var httpClient = new PrismicHttpClient(new HttpClient());

            if (settings == null)
                return new DefaultPrismicApiAccessor(
                    httpClient,
                    logger,
                    inMemoryCache
                );

            return new DefaultPrismicApiAccessor(
                    httpClient,
                    logger,
                    inMemoryCache,
                    Options.Create(settings)
                );
        }

        public static Task<Api> GetApi(string url)
        {
            var accessor = GetAccessor();

            return accessor.GetApi(url);
        }
    }
}
