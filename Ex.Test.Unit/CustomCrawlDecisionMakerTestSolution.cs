using Abot.Core;
using Abot.Poco;
using Ex.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;

namespace Ex.Test.Unit
{
    [TestClass]
    public class CustomCrawlDecisionMakerTestSolution
    {
        CustomCrawlDecisionMaker _unitUnderTest;
        CrawlContext _crawlContext;

        [TestInitialize]
        public void SetUp()
        {
            _crawlContext = new CrawlContext();
            _crawlContext.CrawlConfiguration = new CrawlConfiguration { UserAgentString = "aaa" };
            _unitUnderTest = new CustomCrawlDecisionMaker();
        }


        [TestMethod]
        public void ShouldCrawlPage_NullPageToCrawl_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(null, _crawlContext);

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Null page to crawl", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_NullCrawlContext_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("http://a.com/")), null);

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Null crawl context", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_HappyPath_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(
                new PageToCrawl(new Uri("http://a.com/"))
                {
                    IsInternal = true
                },
                _crawlContext);

            Assert.IsTrue(result.Allow);
            Assert.AreEqual("", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);

        }

        [TestMethod]
        public void ShouldCrawlPage_NonHttpOrHttpsSchemes_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("file:///C:/Users/")), _crawlContext);
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Scheme does not begin with http", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);

            result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("mailto:user@yourdomainname.com")), _crawlContext);
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Scheme does not begin with http", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);

            result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("ftp://user@yourdomainname.com")), _crawlContext);
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Scheme does not begin with http", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);

            result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("callto:+1234567")), _crawlContext);
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Scheme does not begin with http", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);

            result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("tel:+1234567")), _crawlContext);
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Scheme does not begin with http", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_OverMaxPageToCrawlLimit_ReturnsFalse()
        {
            _crawlContext.CrawlConfiguration.MaxPagesToCrawl = 100;
            _crawlContext.CrawledCount = 100;

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("http://a.com/b")) { IsInternal = true }, _crawlContext);

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("MaxPagesToCrawl limit of [100] has been reached", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_OverMaxPageToCrawlLimitByScheduler_ReturnsFalse()
        {
            _crawlContext.CrawlConfiguration.MaxPagesToCrawl = 100;
            _crawlContext.CrawledCount = 100;

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("http://a.com/b")) { IsInternal = true }, _crawlContext);

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("MaxPagesToCrawl limit of [100] has been reached", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_ZeroMaxPageToCrawlLimit_ReturnsTrue()
        {
            CrawlContext crawlContext = new CrawlContext
            {
                CrawlConfiguration = new CrawlConfiguration
                {
                    MaxPagesToCrawl = 0
                },
                CrawledCount = 100
            };

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("http://a.com/")) { IsInternal = true }, crawlContext);

            Assert.IsTrue(result.Allow);
        }

        [TestMethod]
        public void ShouldCrawlPage_IsExternalPageCrawlingEnabledTrue_PageIsExternal_ReturnsTrue()
        {
            _crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled = true;
            _crawlContext.CrawlStartDate = DateTime.Now;

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(
                new PageToCrawl(new Uri("http://a.com/"))
                {
                    IsInternal = false
                },
                _crawlContext);

            Assert.IsTrue(result.Allow);
            Assert.AreEqual("", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_IsExternalPageCrawlingEnabledFalse_PageIsInternal_ReturnsTrue()
        {
            _crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled = false;
            _crawlContext.CrawlStartDate = DateTime.Now;

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(
                new PageToCrawl(new Uri("http://a.com/"))
                {
                    IsInternal = true
                },
                _crawlContext);

            Assert.IsTrue(result.Allow);
            Assert.AreEqual("", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_IsExternalPageCrawlingEnabledTrue_PageIsInternal_ReturnsTrue()
        {
            _crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled = true;
            _crawlContext.CrawlStartDate = DateTime.Now;

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(
                new PageToCrawl(new Uri("http://a.com/"))
                {
                    IsInternal = true
                },
                _crawlContext);

            Assert.IsTrue(result.Allow);
            Assert.AreEqual("", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_OverMaxPagesToCrawlPerDomain_IsRetry_ReturnsTrue()
        {
            Uri uri = new Uri("http://a.com/");
            CrawlConfiguration config = new CrawlConfiguration
            {
                MaxPagesToCrawlPerDomain = 100
            };
            ConcurrentDictionary<string, int> countByDomain = new ConcurrentDictionary<string, int>();
            countByDomain.TryAdd(uri.Authority, 100);
            CrawlContext crawlContext = new CrawlContext
            {
                CrawlConfiguration = config,
                CrawlStartDate = DateTime.Now,
                CrawlCountByDomain = countByDomain
            };

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(
                new PageToCrawl(new Uri(uri.AbsoluteUri + "anotherpage"))
                {
                    IsRetry = true,
                    IsInternal = true
                },
                crawlContext);

            Assert.IsTrue(result.Allow);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_OverMaxCrawlDepth_ReturnsFalse()
        {
            CrawlContext crawlContext = new CrawlContext
            {
                CrawlConfiguration = new CrawlConfiguration
                {
                    MaxCrawlDepth = 2
                }
            };

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(
                new PageToCrawl(new Uri("http://a.com/"))
                {
                    IsInternal = true,
                    CrawlDepth = 3
                },
                crawlContext);

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Crawl depth is above max", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPage_EqualToMaxCrawlDepth_ReturnsTrue()
        {
            _crawlContext.CrawlConfiguration.MaxCrawlDepth = 2;

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(
                new PageToCrawl(new Uri("http://a.com/"))
                {
                    IsInternal = true,
                    CrawlDepth = 2
                },
                _crawlContext);

            Assert.IsTrue(result.Allow);
            Assert.AreEqual("", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }


        [TestMethod]
        public void ShouldCrawlPageLinks_NullCrawledPage_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(null, new CrawlContext());

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Null crawled page", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_NullCrawlContext_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(new CrawledPage(new Uri("http://a.com/a.html"))
            {
                Content = new PageContent
                {
                    Text = "aaaa"
                }
            }, null);

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Null crawl context", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_NullHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(new CrawledPage(new Uri("http://a.com/"))
            {
                Content = new PageContent
                {
                    Text = null
                }
            }, new CrawlContext());

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Page has no content", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_WhitespaceHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(new CrawledPage(new Uri("http://a.com/"))
            {
                Content = new PageContent
                {
                    Text = "     "
                }
            }, new CrawlContext());

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Page has no content", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_EmptyHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(new CrawledPage(new Uri("http://a.com/"))
            {
                Content = new PageContent
                {
                    Text = ""
                }
            }, new CrawlContext());

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Page has no content", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_IsExternalPageLinksCrawlingEnabledFalse_ExternalLink_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    Content = new PageContent
                    {
                        Text = "aaaa"
                    },
                    IsInternal = false
                },
                new CrawlContext
                {
                    RootUri = new Uri("http://a.com/ "),
                    CrawlConfiguration = new CrawlConfiguration
                    {
                        IsExternalPageLinksCrawlingEnabled = false
                    }
                });
            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Link is external", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_IsExternalPageLinksCrawlingEnabledTrue_InternalLink_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    Content = new PageContent
                    {
                        Text = "aaaa"
                    },
                    IsInternal = true
                },
                new CrawlContext
                {
                    RootUri = new Uri("http://a.com/ "),
                    CrawlConfiguration = new CrawlConfiguration
                    {
                        IsExternalPageLinksCrawlingEnabled = true
                    }
                });
            Assert.AreEqual(true, result.Allow);
            Assert.AreEqual("", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_IsExternalPageLinksCrawlingEnabledFalse_InternalLink_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    Content = new PageContent
                    {
                        Text = "aaaa"
                    },
                    IsInternal = true
                },
                new CrawlContext
                {
                    RootUri = new Uri("http://a.com/ "),
                    CrawlConfiguration = new CrawlConfiguration
                    {
                        IsExternalPageLinksCrawlingEnabled = false
                    }
                });
            Assert.AreEqual(true, result.Allow);
            Assert.AreEqual("", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_IsEqualToMaxCrawlDepth_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    Content = new PageContent
                    {
                        Text = "aaaa"
                    },
                    IsInternal = true,
                    CrawlDepth = 2
                },
                new CrawlContext
                {
                    RootUri = new Uri("http://a.com/ "),
                    CrawlConfiguration = new CrawlConfiguration
                    {
                        MaxCrawlDepth = 2
                    }
                });
            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Crawl depth is above max", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldCrawlPageLinks_IsAboveMaxCrawlDepth_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    Content = new PageContent
                    {
                        Text = "aaaa"
                    },
                    IsInternal = true,
                    CrawlDepth = 3
                },
                new CrawlContext
                {
                    RootUri = new Uri("http://a.com/ "),
                    CrawlConfiguration = new CrawlConfiguration
                    {
                        MaxCrawlDepth = 2
                    }
                });
            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Crawl depth is above max", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }


        [TestMethod]
        public void ShouldDownloadPageContent_NullCrawledPage_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(null, new CrawlContext());
            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Null crawled page", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldDownloadPageContent_NullCrawlContext_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(new CrawledPage(new Uri("http://a.com/a.html")), null);

            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Null crawl context", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldDownloadPageContent_NullHttpWebResponse_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    HttpWebResponse = null
                }, 
                _crawlContext);

            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Null HttpWebResponse", result.Reason);
            Assert.IsFalse(result.ShouldHardStopCrawl);
            Assert.IsFalse(result.ShouldStopCrawl);
        }

        [TestMethod]
        public void ShouldDownloadPageContent_ValidHttpWebResponse_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(
                new CrawledPage(new Uri("http://b.com/a.html")){
                    HttpWebResponse = GetValidPage().HttpWebResponse
                }, 
                _crawlContext);

            Assert.AreEqual(true, result.Allow);
        }


        private CrawledPage GetValidPage()
        {
            return new PageRequester(_crawlContext.CrawlConfiguration).MakeRequest(new Uri("http://yahoo.com"));
        }
    }
}
