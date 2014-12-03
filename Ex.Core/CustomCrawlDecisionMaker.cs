using Abot.Core;
using Abot.Poco;

namespace Ex.Core
{
    public class CustomCrawlDecisionMaker : ICrawlDecisionMaker
    {

        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (pageToCrawl == null)
                return new CrawlDecision { Allow = false, Reason = "Null page to crawl" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };

            if (pageToCrawl.CrawlDepth > crawlContext.CrawlConfiguration.MaxCrawlDepth)
                return new CrawlDecision { Allow = false, Reason = "Crawl depth is above max" };

            if (!pageToCrawl.Uri.Scheme.StartsWith("http"))
                return new CrawlDecision { Allow = false, Reason = "Scheme does not begin with http" };

            if (crawlContext.CrawlConfiguration.MaxPagesToCrawl > 0 && crawlContext.CrawledCount >= crawlContext.CrawlConfiguration.MaxPagesToCrawl)
                return new CrawlDecision { Allow = false, Reason = "MaxPagesToCrawl limit of [100] has been reached" };

            return new CrawlDecision { Allow = true };
        }

        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawled page" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };

            if (string.IsNullOrWhiteSpace(crawledPage.Content.Text))
                return new CrawlDecision { Allow = false, Reason = "Page has no content" };

            if (!crawlContext.CrawlConfiguration.IsExternalPageLinksCrawlingEnabled && !crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "Link is external" };

            if (crawledPage.CrawlDepth >= crawlContext.CrawlConfiguration.MaxCrawlDepth)
                return new CrawlDecision { Allow = false, Reason = "Crawl depth is above max" };

            return new CrawlDecision { Allow = true };
        }

        public CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawled page" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };

            if (crawledPage.HttpWebResponse == null)
                return new CrawlDecision { Allow = false, Reason = "Null HttpWebResponse" };

            return new CrawlDecision { Allow = true };    
        }
    }
}
