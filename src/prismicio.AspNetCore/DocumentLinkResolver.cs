using System;

namespace prismic
{
    public abstract class DocumentLinkResolver
    {
        public abstract string Resolve(fragments.DocumentLink link);

        public string Resolve(Document doc) => Resolve(doc.AsDocumentLink());

        #pragma warning disable IDE0060
        public string GetTitle(fragments.DocumentLink link) => null;
        #pragma warning restore IDE0060

        public static DocumentLinkResolver For(Func<fragments.DocumentLink, string> resolver) => new LambdaDocumentLinkResolver(resolver);
    }

    public class LambdaDocumentLinkResolver : DocumentLinkResolver
    {
        private readonly Func<fragments.DocumentLink, string> _f;

        public override string Resolve(fragments.DocumentLink link) => _f(link);

        public LambdaDocumentLinkResolver(Func<fragments.DocumentLink, string> f) => _f = f;

    }
}
