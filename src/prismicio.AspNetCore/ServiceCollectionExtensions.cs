using prismic;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PrismicServiceCollectionExtensions
    {
        public static IServiceCollection AddPrismic(this IServiceCollection services)
        {
            HttpServiceCollectionExtensions.AddHttpContextAccessor(services);
            services.AddPrismic<HttpContextAwarePrismicApiAccessor>();
            return services;
        }

        public static IServiceCollection AddPrismic<TPrismicApiAccessor>(this IServiceCollection services)
           where TPrismicApiAccessor : class, IPrismicApiAccessor
        {
            services.TryAddSingleton<ICache, InMemoryCache>();
            services.AddHttpClient<PrismicHttpClient>();
            services.TryAddSingleton<IPrismicApiAccessor, TPrismicApiAccessor>();

            return services;
        }
    }
}