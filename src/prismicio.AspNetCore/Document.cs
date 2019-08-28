using System.Net;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;

namespace prismic
{
    public class Document : WithFragments
    {
        public string Id { get; }
        public string Uid { get; }
        public string Href { get; }
        public ISet<string> Tags { get; }
        public IList<string> Slugs { get; }
        public string Slug => Slugs.Count > 0 ? Slugs[0] : "-";
        public string Type { get; }
        public DateTime? FirstPublicationDate { get; }
        public DateTime? LastPublicationDate { get; }

        public Document(string id, string uid, string type, string href, ISet<string> tags, IList<string> slugs, IDictionary<string, Fragment> fragments, DateTime? firstPublicationDate, DateTime? lastPublicationDate)
            : base(fragments)
        {
            Id = id;
            Uid = uid;
            Type = type;
            Href = href;
            Tags = tags;
            Slugs = slugs;
            FirstPublicationDate = firstPublicationDate;
            LastPublicationDate = lastPublicationDate;
        }

        public fragments.DocumentLink AsDocumentLink() => new fragments.DocumentLink(Id, Uid, Type, Tags, Slugs[0], this.Fragments, false);

        public static IDictionary<string, Fragment> ParseFragments(JToken json)
        {
            IDictionary<string, Fragment> fragments = new Dictionary<string, Fragment>();

            if (json == null)
            {
                return fragments;
            }
            var type = (string)json["type"];

            if (json["data"] == null)
            {
                return fragments;
            }
            foreach (var field in ((JObject)json["data"][type]))
            {
                var fragmentName = $"{type}.{field.Key}";
                if (field.Value is JArray)
                {
                    var i = 0;
                    foreach (JToken elt in ((JArray)field.Value))
                    {
                        fragmentName = type + "." + field.Key + "[" + i++ + "]";
                        AddFragment(fragments, $"{fragmentName}[{i++}]", MapFragment(elt));
                    }
                }
                else
                {
                    AddFragment(fragments, fragmentName, MapFragment(field.Value));
                }
            }
            return fragments;
        }

        private static Fragment MapFragment(JToken field)
            => prismic.fragments.FragmentParser.Parse((string)field["type"], field["value"]);

        private static void AddFragment(IDictionary<string, Fragment> fragments, string name, Fragment fragment)
        {
            if (fragment == null)
                return;

            fragments[name] = fragment;
        }

        public static Document Parse(JToken json)
        {
            var id = (string)json["id"];
            var uid = (string)json["uid"];
            var href = (string)json["href"];
            var type = (string)json["type"];
            var firstPublicationDate = (DateTime?)json["first_publication_date"];
            var lastPublicationDate = (DateTime?)json["last_publication_date"];

            var tags = new HashSet<string>(json["tags"].Select(r => (string)r));
            var slugs = json["slugs"].Select(r => WebUtility.UrlDecode((string)r)).ToList();
            var fragments = ParseFragments(json);

            return new Document(id, uid, type, href, tags, slugs, fragments, firstPublicationDate, lastPublicationDate);
        }
    }
}
