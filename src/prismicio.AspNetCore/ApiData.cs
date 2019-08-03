using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace prismic
{
    public class ApiData
    {
        public IList<Ref> Refs { get; }
        public IDictionary<string, string> Bookmarks { get; }
        public IDictionary<string, string> Types { get; }
        public IList<string> Tags { get; }
        public IDictionary<string, Form> Forms { get; }
        public string OAuthInitiateEndpoint { get; }
        public string OAuthTokenEndpoint { get; }
        public Experiments Experiments { get; }

        public ApiData(IList<Ref> refs,
            IDictionary<string, string> bookmarks,
            IDictionary<string, string> types,
            IList<string> tags,
            IDictionary<string, Form> forms,
            Experiments experiments,
            string oauthInitiateEndpoint,
            string oauthTokenEndpoint)
        {
            Refs = refs;
            Bookmarks = bookmarks;
            Types = types;
            Tags = tags;
            Forms = forms;
            Experiments = experiments;
            OAuthInitiateEndpoint = oauthInitiateEndpoint;
            OAuthTokenEndpoint = oauthTokenEndpoint;
        }

        public static ApiData Parse(JToken json)
        {
            var refs = json["refs"].Select(r => Ref.Parse((JObject)r)).ToList();
            var tags = json["tags"].Select(r => (string)r).ToList();

            var bookmarks = Map<string>(json, "bookmarks", b => (string)b);
            var types = Map<string>(json, "types", t => (string)t);
            var forms = Map<Form>(json, "forms", f => Form.Parse((JObject)f));

            var oauthInitiateEndpoint = (string)json["oauth_initiate"];
            var oauthTokenEndpoint = (string)json["oauth_token"];

            var experiments = Experiments.Parse(json["experiments"]);

            return new ApiData(refs, bookmarks, types, tags, forms, experiments, oauthInitiateEndpoint, oauthTokenEndpoint);
        }

        private static Dictionary<string, T> Map<T>(JToken json, string key, Func<JToken, T> mapper)
        {
            var source = (JObject)json[key];
            var dest = new Dictionary<string, T>();

            foreach (KeyValuePair<string, JToken> pair in source)
            {
                dest[pair.Key] = mapper(pair.Value);
            }

            return dest;
        }

    }

}

