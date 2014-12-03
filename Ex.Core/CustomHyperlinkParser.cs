using Abot.Core;
using Abot.Poco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ex.Core
{
    public class CustomHyperlinkParser : IHyperLinkParser
    {
        public IEnumerable<Uri> GetLinks(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                throw new ArgumentNullException("crawledPage");

            if (crawledPage.Content != null && !string.IsNullOrWhiteSpace(crawledPage.Content.Text))
            {
                return crawledPage.CsQueryDocument
                    .Select("a, area")
                    .Elements
                    .Select(h => new Uri(crawledPage.Uri, h.GetAttribute("href")));
            }

            return new List<Uri>();
        }
    }
}
