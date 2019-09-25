using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace prismic
{
    public class PrismicHttpClient
    {
        private readonly HttpClient _client;

        public PrismicHttpClient(HttpClient client)
        {
            _client = client;
        }

        public Task<JToken> Fetch(string url, ILogger logger, ICache cache)
        {
            JToken fromCache = cache.Get(url);
            if (fromCache != null)
            {
                return Task.FromResult(fromCache);
            }
            else
            {
                return FetchImpl(url, logger, cache);
            }
        }

        private static readonly Regex maxAgeRe = new Regex(@"max-age=(\d+)");

        private async Task<JToken> FetchImpl(string url, ILogger logger, ICache cache)
        {
            using (var response = await _client.GetAsync(url))
            {
                var body = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var json = JToken.Parse(body);
                        var maxAgeValue = "";
                        IEnumerable<string> maxAgeValues;
                        if (response.Headers.TryGetValues("max-age", out maxAgeValues))
                        {
                            maxAgeValue = maxAgeValues.FirstOrDefault();
                        }
                        var maxAge = maxAgeRe.Match(maxAgeValue);
                        if (maxAge.Success)
                        {
                            long ttl = long.Parse(maxAge.Groups[1].Value);
                            cache.Set(url, ttl, json);
                        }
                        return json;
                    case HttpStatusCode.Unauthorized:
                        var errorText = (string)JObject.Parse(body)["error"];
                        if (errorText == "Invalid access token")
                        {
                            throw new Error(Error.ErrorCode.INVALID_TOKEN, errorText);
                        }
                        else
                        {
                            throw new Error(Error.ErrorCode.AUTHORIZATION_NEEDED, errorText);
                        }
                    default:
                        logger.LogError("Prismic API returned an unexpected {statusCode}", response.StatusCode);
                        throw new Error(Error.ErrorCode.UNEXPECTED, body);
                }
            }
        }
    }
}
