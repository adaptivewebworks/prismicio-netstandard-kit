using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace prismic.AspNetCore.Tests
{
    public class Fixtures
    {
        public static JToken Get(String file)
        {
            var directory = Directory.GetCurrentDirectory();
            var sep = Path.DirectorySeparatorChar;
            var path = $"{directory}{sep}fixtures{sep}{file}";
            string text = System.IO.File.ReadAllText(path);
            return JToken.Parse(text);
        }

        public static Document GetDocument(String file)
        {
            var json = Get(file);
            return Document.Parse(json);
        }
    }
}
