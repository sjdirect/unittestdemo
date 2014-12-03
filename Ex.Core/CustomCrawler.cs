using Abot.Core;
using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;

namespace Ex.Core
{
    public interface ICrawler
    {
        int CrawlCount { get; }
        void Crawl(PageToCrawl root);
    }

    public class CustomCrawler : ICrawler
    {
        ICrawlDecisionMaker _decisionMaker;
        IPageRequester _pageRequester;
        IHyperLinkParser _hyperlinkParser;
        ILog _logger;

        int _pagesCrawled;

        public int CrawlCount { get; set; }

        public CustomCrawler(ICrawlDecisionMaker decisionMaker, IPageRequester pageRequester, IHyperLinkParser hyperlinkParser, ILog logger)
        {
            _decisionMaker = decisionMaker;
            _pageRequester = pageRequester;
            _hyperlinkParser = hyperlinkParser;
            _logger = logger;
        }

        public void Crawl(PageToCrawl pageToCrawl)
        {
            if(_decisionMaker.ShouldCrawlPage(pageToCrawl, null).Allow)
            {
                CrawlCount++;

                CrawledPage crawledPage = null;
                
                try
                {
                    crawledPage = _pageRequester.MakeRequest(pageToCrawl.Uri);
                }
                catch(Exception e)
                {
                    _logger.Error(e.Message);
                }

                if(crawledPage != null && _decisionMaker.ShouldCrawlPageLinks(crawledPage, null).Allow)
                {
                    IEnumerable<Uri> links = _hyperlinkParser.GetLinks(crawledPage);
                    foreach (Uri link in links)
                    {
                        Crawl(new PageToCrawl(link));
                    }
                }
            }
        }
    }
}
