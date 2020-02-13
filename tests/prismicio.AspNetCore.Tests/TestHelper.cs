using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace prismic.AspNetCore.Tests
{
    public static class TestHelper
    {
        public static readonly string Endpoint = "https://apsnet-core-sdk.cdn.prismic.io/api";
        public static DefaultPrismicApiAccessor GetDefaultAccessor(PrismicSettings settings = null)
            => CreatePrismicApiAccessor((sp, httpClient, logger, cache) =>
        {
            if (settings == null)
                return new DefaultPrismicApiAccessor(
                    httpClient,
                    logger,
                    cache
                );

            return new DefaultPrismicApiAccessor(
                    httpClient,
                    logger,
                    cache,
                    Options.Create(settings)
                );
        });

        public static HttpContextAwarePrismicApiAccessor GetHttpContextAwareAccessor(PrismicSettings settings = null)
            => CreatePrismicApiAccessor((sp, httpClient, logger, cache) =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();

                if (settings == null)
                    return new HttpContextAwarePrismicApiAccessor(
                        httpClient,
                        logger,
                        cache,
                        httpContextAccessor
                    );

                return new HttpContextAwarePrismicApiAccessor(
                        httpClient,
                        logger,
                        cache,
                        Options.Create(settings),
                        httpContextAccessor
                    );
            });

        public static TPrismicApiAccessor CreatePrismicApiAccessor<TPrismicApiAccessor>(Func<IServiceProvider, PrismicHttpClient, ILogger<Api>, ICache, TPrismicApiAccessor> builder)
            where TPrismicApiAccessor : class, IPrismicApiAccessor
        {
            var serviceProvider = GetServiceCollection().AddHttpContextAccessor().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<Api>();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new InMemoryCache(memoryCache);
            var httpClient = new PrismicHttpClient(new HttpClient());

            return builder(serviceProvider, httpClient, logger, cache);
        }

        private static IServiceCollection GetServiceCollection()
            => new ServiceCollection().AddLogging();

        public static Task<Api> GetApi(string url)
        {
            var accessor = GetDefaultAccessor();

            return accessor.GetApi(url);
        }
    }
}
