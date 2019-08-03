﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using prismic.fragments;

namespace prismic
{
    public abstract class WithFragments
    {
        public IDictionary<string, Fragment> Fragments { get; }

        public WithFragments(IDictionary<string, Fragment> fragments)
        {
            Fragments = fragments;
        }

        public IList<Fragment> GetAll(string field)
        {
            Regex r = new Regex(Regex.Escape(field) + @"\[\d+\]");
            // TODO test this...
            // return Fragments
            //     .Where(f => r.Match(f.Key).Success)
            //     .Select(f => f.Value)
            //     .ToList();
            IList<Fragment> result = new List<Fragment>();
            foreach (KeyValuePair<string, Fragment> entry in Fragments)
            {
                if (r.Match(entry.Key).Success)
                {
                    result.Add(entry.Value);
                }
            }
            return result;
        }

        public Fragment Get(string field)
        {
            if (!Fragments.TryGetValue(field, out Fragment single))
                return null;

            IList<Fragment> multi = GetAll(field);
            if (multi.Count > 0)
            {
                return multi[0];
            }
            return single;
        }

        public string GetText(string field)
        {
            Fragment frag = Get(field);
            if (frag is Text text)
            {
                return text.Value;
            }
            if (frag is Number number)
            {
                return number.Value.ToString();
            }
            if (frag is Color color)
            {
                return color.Hex;
            }
            if (frag is StructuredText sturcturedText)
            {
                var result = "";
                foreach (StructuredText.Block block in sturcturedText.Blocks)
                {
                    if (block is StructuredText.TextBlock textBlock)
                    {
                        result += textBlock.Text;
                    }
                }
                return result;
            }
            if (frag is Number number1)
            {
                return number1.Value.ToString();
            }
            return null;
        }

        public Number GetNumber(string field)
        {
            Fragment frag = Get(field);
            return frag is Number ? (Number)frag : null;
        }

        public SliceZone GetSliceZone(string field)
        {
            Fragment frag = Get(field);
            return frag is SliceZone ? (SliceZone)frag : null;
        }

        public Image.View GetImageView(string field, string view)
        {
            var image = GetImage(field);
            if (image != null)
                return image.GetView(view);
            return null;
        }

        public Image GetImage(string field)
        {
            Fragment frag = Get(field);
            return frag is Image image ? image : null;
        }

        public Link GetLink(string field)
        {
            Fragment frag = Get(field);
            return frag is Link link ? link : null;
        }

        public Date GetDate(string field)
        {
            Fragment frag = Get(field);
            return frag is Date date ? date : null;
        }

        public Timestamp GetTimestamp(string field)
        {
            Fragment frag = Get(field);
            return frag is Timestamp timestamp ? timestamp : null;
        }

        public Embed GetEmbed(string field)
        {
            Fragment frag = Get(field);
            return frag is Embed ? (Embed)frag : null;
        }

        public fragments.Group GetGroup(string field)
        {
            Fragment frag = Get(field);
            return frag is fragments.Group ? (fragments.Group)frag : null;
        }

        public Color GetColor(string field)
        {
            Fragment frag = Get(field);
            return frag is Color color ? color : null;
        }

        public GeoPoint GetGeoPoint(string field)
        {
            Fragment frag = Get(field);
            return frag is GeoPoint ? (GeoPoint)frag : null;
        }

        public StructuredText GetStructuredText(string field)
        {
            Fragment frag = Get(field);
            return frag is StructuredText ? (StructuredText)frag : null;
        }

        public string GetHtml(string field, DocumentLinkResolver resolver)
        {
            return GetHtml(field, resolver, null);
        }

        public string GetHtml(string field, DocumentLinkResolver resolver, HtmlSerializer serializer)
        {
            Fragment fragment = Get(field);
            return GetHtml(fragment, resolver, serializer);
        }

        public static string GetHtml(Fragment fragment, DocumentLinkResolver resolver, HtmlSerializer serializer)
        {
            if (fragment == null)
                return string.Empty;

            switch (fragment)
            {
                case StructuredText structuredText:
                    return structuredText.AsHtml(resolver, serializer);
                case Number number:
                    return number.AsHtml();
                case Color color:
                    return color.AsHtml();
                case Text text:
                    return text.AsHtml();
                case Date date:
                    return date.AsHtml();
                case Embed embed:
                    return embed.AsHtml();
                case Image image:
                    return image.AsHtml(resolver);
                case WebLink webLink:
                    return webLink.AsHtml();
                case DocumentLink docLink:
                    return docLink.AsHtml(resolver);
                case fragments.Group group:
                    return group.AsHtml(resolver);
                case SliceZone zone:
                    return zone.AsHtml(resolver);
                default:
                    return string.Empty;
            }
        }

        public string AsHtml(DocumentLinkResolver linkResolver) => AsHtml(linkResolver, null);

        public string AsHtml(DocumentLinkResolver linkResolver, HtmlSerializer htmlSerializer)
        {
            string html = "";
            foreach (KeyValuePair<string, Fragment> fragment in Fragments)
            {
                html += ("<section data-field=\"" + fragment.Key + "\">");
                html += GetHtml(fragment.Key, linkResolver, htmlSerializer);
                html += ("</section>");
            }
            return html.Trim();
        }

        public IList<DocumentLink> LinkedDocuments()
        {
            var result = new List<DocumentLink>();
            foreach (Fragment fragment in Fragments.Values)
            {
                if (fragment is DocumentLink dl)
                {
                    result.Add(dl);
                }
                else if (fragment is StructuredText text)
                {
                    foreach (StructuredText.Block block in text.Blocks)
                    {
                        if (block is StructuredText.TextBlock)
                        {
                            var spans = ((StructuredText.TextBlock)block).Spans;
                            foreach (StructuredText.Span span in spans)
                            {
                                if (span is StructuredText.Hyperlink)
                                {
                                    var link = ((StructuredText.Hyperlink)span).Link;
                                    if (link is DocumentLink docLink)
                                    {
                                        result.Add(docLink);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (fragment is fragments.Group group)
                {
                    result.AddRange(group.GroupDocs.SelectMany(d => d.LinkedDocuments()));
                }
            }
            return result;
        }

        public Raw GetRaw(string field)
        {
            Fragment frag = Get(field);
            return frag is Raw ? (Raw)frag : null;
        }
    }
}

