using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddPrismic_adds_required_dependencies()
        {
            var collection = new ServiceCollection()
                .AddLogging();
        
            collection.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
            collection.AddPrismic();
             var serviceProvider = collection.BuildServiceProvider();
             
             var cache = serviceProvider.GetService<ICache>();
            Assert.Equal(typeof(InMemoryCache), cache.GetType());

            var client = serviceProvider.GetService<PrismicHttpClient>();
            Assert.NotNull(client);
            Assert.Equal(typeof(PrismicHttpClient), client.GetType());

            var accessor = serviceProvider.GetService<IPrismicApiAccessor>();
            Assert.NotNull(accessor);
            Assert.Equal(typeof(DefaultPrismicApiAccessor), accessor.GetType());
        }

        [Fact]
        public void AddPrismic_with_custom_implementation_does_not_override_implementation()
        {
            var collection = new ServiceCollection();
        
            collection.AddSingleton<ICache, TestCache>();
            collection.AddPrismic();
            var serviceProvider = collection.BuildServiceProvider();
            
            var cache = serviceProvider.GetService<ICache>();
            Assert.NotNull(cache);
            Assert.Equal(typeof(TestCache), cache.GetType());

            cache = serviceProvider.GetService<TestCache>();
            Assert.Null(cache);
        }

        private class TestCache : ICache
        {
            public JToken Get(string key)
            {
                throw new System.NotImplementedException();
            }

            public void Set(string key, long ttl, JToken item)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
