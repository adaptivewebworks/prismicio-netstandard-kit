using prismic;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection {
    public static class ServiceCollectionExtensions {
        
        public static IServiceCollection AddPrismic(this IServiceCollection services) {

            services.TryAddSingleton<ICache, InMemoryCache>();
            services.AddHttpClient<PrismicHttpClient>();
            services.TryAddSingleton<IPrismicApiAccessor, DefaultPrismicApiAccessor>();

            return services;
        }
    }
}