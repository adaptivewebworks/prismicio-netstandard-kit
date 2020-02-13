using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using prismic;

namespace prismicio.AspNetCore.MvcSample.Controllers
{
    public class MvcSample : Controller
    {
        private readonly IPrismicApiAccessor _prismic;
        public MvcSample(IPrismicApiAccessor prismic)
        {
            _prismic = prismic;
        }

        [Route("~/mvc")]
        public async Task<IActionResult> Index()
        {
            var api = await _prismic.GetApi();
            
            var docType = "test_document";

            var document = await api.GetByUID(docType, "test-document");

            return View(model: document.GetText($"{docType}.text"));
        }

    }
}