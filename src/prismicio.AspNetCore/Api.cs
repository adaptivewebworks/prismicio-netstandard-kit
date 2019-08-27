using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace prismic
{
    public class Api
    {
        public const string PREVIEW_COOKIE = "io.prismic.preview";
        public const string EXPERIMENT_COOKIE = "io.prismic.experiment";

        private readonly PrismicHttpClient PrismicHttpClient;
        private readonly ICache cache;
        private readonly ILogger logger;
        private readonly ApiData apiData;

        public IList<Ref> Refs => apiData.Refs;
        public IDictionary<string, Form> Forms => apiData.Forms;

        public IDictionary<string, string> Bookmarks => apiData.Bookmarks;

        public IDictionary<string, string> Types => apiData.Types;

        public IList<string> Tags => apiData.Tags;

        public Experiments Experiments => apiData.Experiments;

        public Api(ApiData apiData, ICache cache, ILogger<prismic.Api> logger, PrismicHttpClient client)
        {
            this.apiData = apiData;
            this.cache = cache;
            this.logger = logger;
            this.PrismicHttpClient = client;
        }

        public Ref Ref(string label) => Refs.FirstOrDefault(r => r.Label == label);

        public Ref Master => Refs.FirstOrDefault(r => r.IsMasterRef);


        public Form.SearchForm Form(string form) => new Form.SearchForm(this, Forms[form]);

        public Form.SearchForm Query(string q)
            => Form("everything")
                .Ref(this.Master)
                .Query(q);

        public Form.SearchForm Query(params IPredicate[] predicates)
            => Form("everything")
                .Ref(this.Master)
                .Query(predicates);

        /**
         * Retrieve multiple documents from their IDS
         */
        public Form.SearchForm GetByIDs(IEnumerable<string> ids, string reference = null, string lang = null) 
            => Query(Predicates.@in("document.id", ids))
                .Ref(reference)
                .Lang(lang);

        /**
         * Return the first document matching the predicate
         */
        public async Task<Document> QueryFirst(IPredicate p, string reference = null, string lang = null)
        {
            SetReferenceOrMaster(reference);

            var response = await Query(p).Ref(reference).Lang(lang).Submit();
            var results = response.Results;
            if (results.Count() > 0)
            {
                return results[0];
            }
            else
            {
                return null;
            }
        }

        /**
         * Retrieve a document by its ID on the given reference
         *
         * @return the document, or null if it doesn't exist
         */
        public Task<Document> GetByID(string documentId, string reference = null, string lang = null)
        {
            SetReferenceOrMaster(reference);

            return QueryFirst(Predicates.at("document.id", documentId), reference, lang);
        }

        /**
         * Retrieve a document by its UID on the given reference
         *
         * @return the document, or null if it doesn't exist
         */
        public Task<Document> GetByUID(string documentType, string documentUid, string reference = null, string lang = null)
        {
            SetReferenceOrMaster(reference);

            return QueryFirst(Predicates.at("my." + documentType + ".uid", documentUid), reference, lang);
        }

        public Task<Document> GetBookmark(string bookmark, string reference = null)
        {
            SetReferenceOrMaster(reference);

            return GetByID(apiData.Bookmarks[bookmark], reference);
        }

        /**
        * Return the URL to display a given preview
        * @param token as received from Prismic server to identify the content to preview
        * @param linkResolver the link resolver to build URL for your site
        * @param defaultUrl the URL to default to return if the preview doesn't correspond to a document
        *                (usually the home page of your site)
        * @return the URL you should redirect the user to preview the requested change
        */
        public async Task<string> PreviewSession(string token, DocumentLinkResolver linkResolver, string defaultUrl)
        {
            var tokenJson = await this.PrismicHttpClient.Fetch(token, logger, cache);
            var mainDocumentId = tokenJson["mainDocument"];
            if (mainDocumentId == null)
            {
                return (defaultUrl);
            }
            var resp = await Form("everything")
                .Query(Predicates.at("document.id", mainDocumentId.ToString()))
                .Ref(token)
                .Lang()
                .Submit();
            if (resp.Results.Count == 0)
            {
                return defaultUrl;
            }
            return linkResolver.Resolve(resp.Results[0]);
        }

        internal async Task<Response> Fetch(string url)
        {
            logger.LogDebug("Fetching URL: {url}", url);
            var json = await PrismicHttpClient.Fetch(url, logger, cache);
            return Response.Parse(json);
        }

        private string SetReferenceOrMaster(string reference = null)
            => !string.IsNullOrWhiteSpace(reference)
                ? reference
                : Master.Reference;
    }
}

