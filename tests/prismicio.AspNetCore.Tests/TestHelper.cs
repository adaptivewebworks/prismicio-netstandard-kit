using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace prismic.AspNetCore.Tests
{
    public static class TestHelper
    {
        public static Task<Api> GetApi(string url)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddLogging()
                    .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<prismic.Api>();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var accessor = new DefaultPrismicApiAccessor(
                new PrismicHttpClient(new HttpClient()),
                logger,
                new prismic.InMemoryCache(memoryCache)
            );

            return accessor.GetApi(url);
        }
    }
}
