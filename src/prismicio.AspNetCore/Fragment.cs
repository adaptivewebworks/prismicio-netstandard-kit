using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Net;

namespace prismic
{
    public interface Fragment
    {
    }

    namespace fragments
    {

        public class Text : Fragment
        {
            public string Value { get; }
            public Text(string value)
            {
                this.Value = value;
            }

            public string AsHtml() => $"<span class=\"text\">{WebUtility.HtmlEncode(Value)}</span>";
            public static Text Parse(JToken json) => new Text((string)json);
        }

        public class Number : Fragment
        {
            private static readonly CultureInfo _defaultCultureInfo = new CultureInfo("en-US");
            public decimal Value { get; }
            public Number(decimal value)
            {
                this.Value = value;
            }

            public string AsHtml() => $"<span class=\"number\">{Value}</span>";
            public static Number Parse(JToken json, CultureInfo ci = null)
            {
                if (ci == null)
                    ci = _defaultCultureInfo;

                var v = Convert.ToDecimal((string)json, ci);
                return new Number(v);
            }
        }

        public class Image : Fragment
        {
            public class View
            {
                public string Url { get; }
                public int Width { get; }
                public int Height { get; }
                public string Alt { get; }
                public string Copyright { get; }
                public Link LinkTo { get; }

                public double Ratio => Width / Height;

                public View(String url, int width, int height, string alt, string copyright, Link linkTo)
                {
                    Url = url;
                    Width = width;
                    Height = height;
                    Alt = alt;
                    Copyright = copyright;
                    LinkTo = linkTo;
                }

                public string AsHtml(DocumentLinkResolver linkResolver)
                {
                    var imgTag = $"<img alt=\"{Alt}\" src=\"{Url}\" width=\"{Width}\" height=\"{Height}\" />";
                    if (LinkTo != null)
                    {
                        var url = "about:blank";
                        string target = null;
                        if (LinkTo is WebLink webLink)
                        {
                            url = webLink.Url;
                            target = webLink.Target;
                        }
                        else if (LinkTo is ImageLink link)
                        {
                            url = link.Url;
                        }
                        else if (LinkTo is DocumentLink documentLink)
                        {
                            url = documentLink.IsBroken
                                ? "#broken"
                                : linkResolver.Resolve(documentLink);
                        }
                        return HtmlExtensions.Link(url, imgTag, target);
                    }
                    else
                    {
                        return imgTag;
                    }
                }

                //

                public static View Parse(JToken json)
                {
                    string url = (string)json["url"];
                    int width = (int)json["dimensions"]["width"];
                    int height = (int)json["dimensions"]["height"];
                    string alt = (string)json["alt"];
                    string copyright = (string)json["copyright"];
                    Link linkTo = (Link)StructuredText.ParseLink((JObject)json["linkTo"]);
                    return new View(url, width, height, alt, copyright, linkTo);
                }

            }

            private readonly View main;
            private readonly IDictionary<string, View> views;

            public Image(View main, IDictionary<string, View> views)
            {
                this.main = main;
                this.views = views;
            }

            public Image(View main) : this(main, new Dictionary<string, View>()) { }

            public View GetView(string view)
            {
                if ("main" == view)
                {
                    return main;
                }
                return views[view];
            }

            public string AsHtml(DocumentLinkResolver linkResolver) => GetView("main").AsHtml(linkResolver);

            // --

            public static Image Parse(JToken json)
            {
                View main = View.Parse((JObject)json["main"]);
                var views = new Dictionary<string, View>();
                foreach (KeyValuePair<string, JToken> v in ((JObject)json["views"]))
                {
                    views[v.Key] = View.Parse((JObject)v.Value);
                }
                return new Image(main, views);
            }

        }

        public interface Link : Fragment
        {
            string GetUrl(DocumentLinkResolver resolver);
        }

        public class WebLink : Link
        {
            public string Url { get; }
            public string ContentType { get; }
            public string Target { get; }

            public WebLink(string url, string contentType, string target)
            {
                Url = url;
                ContentType = contentType;
                Target = target;
            }

            public string GetUrl(DocumentLinkResolver _) => Url;

            public string AsHtml() => HtmlExtensions.Link(Url, Url, Target);

            public static WebLink Parse(JToken json)
                => new WebLink((string)json["url"], null, (string)json["target"]);
        }

        public class FileLink : Link
        {
            public string Url { get; }
            public string Kind { get; }
            public long Size { get; }
            public string Filename { get; }

            public FileLink(string url, string kind, long size, string filename)
            {
                Url = url;
                Kind = kind;
                Size = size;
                Filename = filename;
            }

            public string GetUrl(DocumentLinkResolver resolver) => Url;

            public static FileLink Parse(JToken json)
            {
                string url = (string)json["file"]["url"];
                string kind = (string)json["file"]["kind"];
                string size = (string)json["file"]["size"];
                string name = (string)json["file"]["name"];
                return new FileLink(url, kind, long.Parse(size), name);
            }

        }

        public class ImageLink : Link
        {
            public string Url { get; }

            public ImageLink(string url)
            {
                Url = url;
            }

            public string GetUrl(DocumentLinkResolver resolver) => Url;

            public string AsHtml() => HtmlExtensions.Link(Url, Url);

            public static ImageLink Parse(JToken json) => new ImageLink((string)json["image"]["url"]);

        }

        public class DocumentLink : WithFragments, Link
        {
            public string Id { get; }
            public string Uid { get; }
            public string Type { get; }
            public ISet<string> Tags { get; }
            public string Slug { get; }
            public bool IsBroken { get; }

            public DocumentLink(string id, string uid, string type, ISet<string> tags, string slug, IDictionary<string, Fragment> fragments, Boolean broken)
                : base(fragments)
            {
                Id = id;
                Uid = uid;
                Type = type;
                Tags = tags;
                Slug = slug;
                IsBroken = broken;
            }

            public string GetUrl(DocumentLinkResolver resolver) => resolver.Resolve(this);

            public new string AsHtml(DocumentLinkResolver linkResolver) => HtmlExtensions.Link(linkResolver.Resolve(this), Slug, title: linkResolver.GetTitle(this));

            public static DocumentLink Parse(JToken json)
            {
                JObject document = (JObject)json["document"];
                Boolean broken = (Boolean)json["isBroken"];
                string id = (string)document["id"];
                string type = (string)document["type"];
                string slug = (string)document["slug"];
                string uid = null;
                if (document["uid"] != null)
                    uid = (string)document["uid"];
                ISet<String> tags;
                if (json["tags"] != null)
                    tags = new HashSet<String>(json["tags"].Select(r => (string)r));
                else
                    tags = new HashSet<String>();
                IDictionary<string, Fragment> fragments = Document.ParseFragments(json["document"]);
                return new DocumentLink(id, uid, type, tags, slug, fragments, broken);
            }

        }

        public class Date : Fragment
        {
            public DateTime Value { get; }
            public Date(DateTime value)
            {
                Value = value;
            }
            public string AsHtml() => $"<time datetime=\"{Value.ToString("o")}\">{Value}</time>";
            public static Date Parse(JToken json)
            {
                try
                {
                    return new Date(DateTime.ParseExact((string)json, "yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
                catch (FormatException)
                {
                    return null;
                }
            }
        }

        public class Timestamp : Fragment
        {
            public DateTime Value { get; }
            public Timestamp(DateTime value)
            {
                Value = value;
            }
            public string AsHtml() => ("<time>" + Value + "</time>");
            public static Timestamp Parse(JToken json) => new Timestamp(json.ToObject<DateTime>());
        }

        public class Embed : Fragment
        {
            public string Type { get; }
            public string Provider { get; }
            public string Url { get; }
            public int? Width { get; }
            public int? Height { get; }
            public string Html { get; }
            public JObject OEmbedJson { get; }

            public Embed(string type, string provider, string url, int? width, int? height, string html, JObject oembedJson)
            {
                Type = type;
                Provider = provider;
                Url = url;
                Width = width;
                Height = height;
                Html = html;
                OEmbedJson = oembedJson;
            }

            public string AsHtml()
            {
                var providerAttr = Provider != null ? ("\" data-oembed-provider=\"" + Provider.ToLower() + "\"") : "";
                return ("<div data-oembed=\"" + Url + "\" data-oembed-type=\"" + Type.ToLower() + "\"" + providerAttr + ">" + Html + "</div>");
            }

            public static Embed Parse(JToken json)
            {
                JObject oembedJson = (JObject)json["oembed"];
                string type = (string)oembedJson["type"];
                string provider = oembedJson["provider_name"] != null ? (string)oembedJson["provider_name"] : null;
                string url = (string)oembedJson["embed_url"];
                int? width = ParseInt(oembedJson, "width");
                int? height = ParseInt(oembedJson, "height");
                string html = (string)oembedJson["html"];
                return new Embed(type, provider, url, width, height, html, oembedJson);
            }

            private static int? ParseInt(JObject json, string key)
            {
                var token = json[key];

                if(token == null || token.Type != JTokenType.Integer)
                    return null;

                return (int?) token;
            }

        }

        public interface Slice : Fragment
        {
            string SliceType { get; }
            string SliceLabel { get; }

            string AsHtml(DocumentLinkResolver resolver);
        }

        public class SimpleSlice : Slice
        {
            public string SliceType { get; }
            public string SliceLabel { get; }
            public Fragment Value { get; }

            public SimpleSlice(string sliceType, string sliceLabel, Fragment value)
            {
                SliceType = sliceType;
                SliceLabel = sliceLabel;
                Value = value;
            }

            public string AsHtml(DocumentLinkResolver resolver)
            {
                var className = "slice";
                if (SliceLabel != null) className += (" " + SliceLabel);
                return "<div data-slicetype=\"" + SliceType + "\" class=\"" + className + "\">" +
                       WithFragments.GetHtml(Value, resolver, null) +
                       "</div>";
            }

        }

        public class CompositeSlice : Slice
        {
            private readonly Group _repeat;
            private readonly GroupDoc _nonRepeat;

            public string SliceType { get; }

            public string SliceLabel { get; }

            public GroupDoc GetPrimary()
            {
                return _nonRepeat;
            }

            public Group GetItems()
            {
                return _repeat;
            }

            public string Items
            {
                get { return SliceLabel; }
            }

            public CompositeSlice(string sliceType, string label, Group repeat, GroupDoc nonRepeat)
            {
                SliceType = sliceType;
                SliceLabel = label;
                _repeat = repeat;
                _nonRepeat = nonRepeat;
            }

            public string AsHtml(DocumentLinkResolver resolver)
            {
                string className = "slice";
                if (SliceLabel != null && SliceLabel != "null")
                    className += (" " + SliceLabel);

                List<GroupDoc> groupDocs = new List<GroupDoc> { _nonRepeat };
                return "<div data-slicetype=\"" + SliceType + "\" class=\"" + className + "\">" +
                            WithFragments.GetHtml(new Group(groupDocs), resolver, null) +
                            WithFragments.GetHtml(_repeat, resolver, null) +
                       "</div>";
            }
        }

        public class SliceZone : Fragment
        {
            public IList<Slice> Slices { get; }

            public SliceZone(IList<Slice> slices)
            {
                Slices = slices;
            }

            public string AsHtml(DocumentLinkResolver linkResolver)
            {
                return Slices.Aggregate("", (html, slice) => html + slice.AsHtml(linkResolver));
            }

            public static SliceZone Parse(JToken json)
            {
                var slices = new List<Slice>();
                foreach (JToken sliceJson in (JArray)json)
                {
                    string sliceType = (string)sliceJson["slice_type"];
                    string label = (string)sliceJson["slice_label"];

                    // Handle Deprecated SliceZones
                    JToken fragJson = sliceJson["value"];
                    if (fragJson != null)
                    {
                        //TODO more parsing refactor opportunities
                        string fragmentType = (string)fragJson["type"];
                        JToken fragmentValue = fragJson["value"];
                        Fragment value = FragmentParser.Parse(fragmentType, fragmentValue);
                        slices.Add(new SimpleSlice(sliceType, label, value));
                    }
                    else
                    {
                        //TODO more parsing refactor opportunities
                        //Parse new format non-repeating slice zones
                        JObject nonRepeatsJson = (JObject)sliceJson["non-repeat"];
                        GroupDoc nonRepeat = GroupDoc.Parse(nonRepeatsJson);
                        JArray repeatJson = (JArray)sliceJson["repeat"];
                        Group repeat = Group.Parse(repeatJson);
                        slices.Add(new CompositeSlice(sliceType, label, repeat, nonRepeat));
                    }
                }
                return new SliceZone(slices);
            }
        }

        public class Group : Fragment
        {
            public IList<GroupDoc> GroupDocs { get; }

            public Group(IList<GroupDoc> groupDocs)
            {
                GroupDocs = groupDocs;
            }

            public string AsHtml(DocumentLinkResolver linkResolver)
            {
                var html = "";
                foreach (GroupDoc groupDoc in this.GroupDocs)
                {
                    html += groupDoc.AsHtml(linkResolver);
                }
                return html;
            }

            public static Group Parse(JToken json)
            {
                var groupDocs = new List<GroupDoc>();
                foreach (JToken groupJson in (JArray)json)
                {
                    // each groupJson looks like this: { "somelink" : { "type" : "Link.document", { ... } }, "someimage" : { ... } }
                    var fragmentMap = new Dictionary<string, Fragment>();
                    foreach (KeyValuePair<string, JToken> field in (JObject)groupJson)
                    {
                        string fragmentType = (string)field.Value["type"];
                        JToken fragmentValue = field.Value["value"];
                        Fragment fragment = FragmentParser.Parse(fragmentType, fragmentValue);
                        if (fragment != null) fragmentMap[field.Key] = fragment;
                    }
                    groupDocs.Add(new GroupDoc(fragmentMap));
                }
                return new Group(groupDocs);
            }

        }

        public class Color : Fragment
        {
            public string Hex { get; }

            public Color(string hex)
            {
                Hex = hex;
            }

            public string AsHtml() => ("<span class=\"color\">" + Hex + "</span>");

            private static readonly Regex r = new Regex(@"#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})");

            public static Color Parse(JToken json)
            {
                string hex = (string)json;
                if (r.Match(hex).Success)
                {
                    return new Color(hex);
                }
                return null;
            }

        }

        public class GeoPoint : Fragment
        {
            private static readonly CultureInfo _defaultCultureInfo = new CultureInfo("en-US");

            public double Latitude { get; }
            public double Longitude { get; }

            public GeoPoint(double latitude, double longitude)
            {
                Latitude = latitude;
                Longitude = longitude;
            }

            public static GeoPoint Parse(JToken json)
            {
                try
                {
                    double latitude = double.Parse((string)json["latitude"], _defaultCultureInfo);
                    double longitude = double.Parse((string)json["longitude"], _defaultCultureInfo);
                    return new GeoPoint(latitude, longitude);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public class Raw : Fragment
        {
            public JToken Value { get; }

            public Raw(JToken json)
            {
                Value = json;
            }

            public string AsText() => Value.ToString();

            public static Raw Parse(JToken json) => new Raw(json);
        }

        public static class FragmentParser
        {
            public static Fragment Parse(string type, JToken json)
            {
                switch (type)
                {
                    case "StructuredText":
                        return StructuredText.Parse(json);
                    case "Image":
                        return Image.Parse(json);
                    case "Link.web":
                        return WebLink.Parse(json);
                    case "Link.document":
                        return DocumentLink.Parse(json);
                    case "Link.file":
                        return FileLink.Parse(json);
                    case "Link.image":
                        return ImageLink.Parse(json);
                    case "Text":
                        return Text.Parse(json);
                    case "Select":
                        return Text.Parse(json);
                    case "Date":
                        return Date.Parse(json);
                    case "Timestamp":
                        return Timestamp.Parse(json);
                    case "Number":
                        return Number.Parse(json);
                    case "Color":
                        return Color.Parse(json);
                    case "Embed":
                        return Embed.Parse(json);
                    case "GeoPoint":
                        return GeoPoint.Parse(json);
                    case "Group":
                        return Group.Parse(json);
                    case "SliceZone":
                        return SliceZone.Parse(json);
                    default:
                        return json != null ? Raw.Parse(json) : null;
                }
            }
        }
    }
}
