using System;
using System.Threading.Tasks;
using Xunit;

namespace prismic.AspNetCore.Tests
{
    public class DefaultPrismicApiAccessorTests
    {


        [Fact]
        public Task Accessor_throws_exception_when_no_settings_provider_and_GetApi_is_called_with_no_arguments()
        {
            var prismic = TestHelper.GetAccessor();

            return Assert.ThrowsAsync<ArgumentNullException>("_settings", () => prismic.GetApi());
        }

        [Fact]
        public Task Accessor_throws_exception_when_endpoint_in_settings_is_invalid_and_GetApi_is_called_with_no_arguments()
        {
            var prismic = TestHelper.GetAccessor(new PrismicSettings());

            return Assert.ThrowsAsync<ArgumentException>("endpoint", () => prismic.GetApi());
        }

        [Fact]
        public async Task Accessor_gets_Api_when_settings_are_valid_and_GetApi_is_called_with_no_arguments()
        {
            var prismic = TestHelper.GetAccessor(new PrismicSettings
            {
                Endpoint = TestHelper.Endpoint
            });
            var api = await prismic.GetApi();
            Assert.NotNull(api.Master);
        }


        [Fact]
        public Task Accessor_throws_exception_when_GetApi_is_called_with_invalid_endpoint()
        {
            var prismic = TestHelper.GetAccessor();
            return Assert.ThrowsAsync<ArgumentException>("endpoint", () => prismic.GetApi(string.Empty));
        }

        [Fact]
        public async Task Accessor_gets_Api_when_GetApi_is_called_with_valid_endpoint()
        {
            var prismic = TestHelper.GetAccessor();
            var api = await prismic.GetApi(TestHelper.Endpoint);
            Assert.NotNull(api.Master);
        }
    }
}
