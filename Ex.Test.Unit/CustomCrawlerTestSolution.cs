using Abot.Core;
using Abot.Poco;
using Ex.Core;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Ex.Test.Unit
{
    [TestClass]
    public class CustomCrawlerTestSolution
    {
        CustomCrawler _uut;
        Mock<ICrawlDecisionMaker> _fakeCrawlDecisionMaker;
        Mock<IPageRequester> _fakePageRequester;
        Mock<IHyperLinkParser> _fakeHyperLinkParser;
        Mock<ILog> _fakeLogger;

        [TestInitialize]
        public void Init()
        {
            _fakeCrawlDecisionMaker = new Mock<ICrawlDecisionMaker>();
            _fakePageRequester = new Mock<IPageRequester>();
            _fakeHyperLinkParser = new Mock<IHyperLinkParser>();
            _fakeLogger = new Mock<ILog>();

            _uut = new CustomCrawler(_fakeCrawlDecisionMaker.Object, _fakePageRequester.Object, _fakeHyperLinkParser.Object, _fakeLogger.Object);
        }

        [TestMethod]
        public void Crawl_DecisionMakerReturnsFalse_NothingIsCrawled()
        {
            //Arrange
            PageToCrawl page = new PageToCrawl(new Uri("http://doesntmatter.com/"));
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(page, null)).Returns(new CrawlDecision { Allow = false, Reason = "aaaa" });

            //Act
            _uut.Crawl(page);

            //Assert
            _fakeCrawlDecisionMaker.VerifyAll();
            Assert.AreEqual(0, _uut.CrawlCount);
        }

        [TestMethod]
        public void Crawl_CrawlerCrawls()
        {
            //Arrange
            PageToCrawl root = new PageToCrawl(new Uri("http://a.com/"));

            CrawledPage rootCrawled = new CrawledPage(new Uri("http://a.com/"));
            CrawledPage page1Crawled = new CrawledPage(new Uri("http://a.com/a1"));
            CrawledPage page2Crawled = new CrawledPage(new Uri("http://a.com/a2"));
            CrawledPage page3Crawled = new CrawledPage(new Uri("http://a.com/a3"));

            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), null)).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), null)).Returns(new CrawlDecision { Allow = true });

            _fakePageRequester.Setup(f => f.MakeRequest(root.Uri)).Returns(rootCrawled);
            _fakePageRequester.Setup(f => f.MakeRequest(page1Crawled.Uri)).Returns(page1Crawled);
            _fakePageRequester.Setup(f => f.MakeRequest(page2Crawled.Uri)).Returns(page2Crawled);
            _fakePageRequester.Setup(f => f.MakeRequest(page3Crawled.Uri)).Returns(page3Crawled);

            _fakeHyperLinkParser.Setup(f => f.GetLinks(rootCrawled)).Returns(new List<Uri>() { page1Crawled.Uri });
            _fakeHyperLinkParser.Setup(f => f.GetLinks(page1Crawled)).Returns(new List<Uri>() { page2Crawled.Uri });
            _fakeHyperLinkParser.Setup(f => f.GetLinks(page2Crawled)).Returns(new List<Uri>() { page3Crawled.Uri });
            _fakeHyperLinkParser.Setup(f => f.GetLinks(page3Crawled)).Returns(new List<Uri>());

            //Act
            _uut.Crawl(root);

            //Assert
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(root, null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.Is<PageToCrawl>(p => p.Uri == page1Crawled.Uri), null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.Is<PageToCrawl>(p => p.Uri == page1Crawled.Uri), null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.Is<PageToCrawl>(p => p.Uri == page1Crawled.Uri), null));

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(rootCrawled, null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(page1Crawled, null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(page2Crawled, null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(page3Crawled, null));

            _fakePageRequester.Verify(f => f.MakeRequest(root.Uri));
            _fakePageRequester.Verify(f => f.MakeRequest(page1Crawled.Uri));
            _fakePageRequester.Verify(f => f.MakeRequest(page2Crawled.Uri));
            _fakePageRequester.Verify(f => f.MakeRequest(page3Crawled.Uri));

            _fakeHyperLinkParser.Verify(f => f.GetLinks(rootCrawled));
            _fakeHyperLinkParser.Verify(f => f.GetLinks(page1Crawled));
            _fakeHyperLinkParser.Verify(f => f.GetLinks(page2Crawled));
            _fakeHyperLinkParser.Verify(f => f.GetLinks(page3Crawled));

            Assert.AreEqual(4, _uut.CrawlCount);
        }

        [TestMethod]
        public void Crawl_ShouldCrawlPageLinksReturnsFalseAfterHomePage_CrawlsOnlyFirstTwoPages()
        {
            //Arrange
            PageToCrawl root = new PageToCrawl(new Uri("http://a.com/"));

            CrawledPage rootCrawled = new CrawledPage(new Uri("http://a.com/"));
            CrawledPage page1Crawled = new CrawledPage(new Uri("http://a.com/a1"));
            CrawledPage page2Crawled = new CrawledPage(new Uri("http://a.com/a2"));
            CrawledPage page3Crawled = new CrawledPage(new Uri("http://a.com/a3"));

            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), null)).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), null)).Returns(new CrawlDecision { Allow = false });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.Is<CrawledPage>(cp => cp.Uri == root.Uri), null)).Returns(new CrawlDecision { Allow = true });

            _fakePageRequester.Setup(f => f.MakeRequest(root.Uri)).Returns(rootCrawled);
            _fakePageRequester.Setup(f => f.MakeRequest(page1Crawled.Uri)).Returns(page1Crawled);
            _fakePageRequester.Setup(f => f.MakeRequest(page2Crawled.Uri)).Returns(page2Crawled);
            _fakePageRequester.Setup(f => f.MakeRequest(page3Crawled.Uri)).Returns(page3Crawled);

            _fakeHyperLinkParser.Setup(f => f.GetLinks(rootCrawled)).Returns(new List<Uri>() { page1Crawled.Uri });
            _fakeHyperLinkParser.Setup(f => f.GetLinks(page1Crawled)).Returns(new List<Uri>() { page2Crawled.Uri });
            _fakeHyperLinkParser.Setup(f => f.GetLinks(page2Crawled)).Returns(new List<Uri>() { page3Crawled.Uri });
            _fakeHyperLinkParser.Setup(f => f.GetLinks(page3Crawled)).Returns(new List<Uri>());

            //Act
            _uut.Crawl(root);

            //Assert
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(root, null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.Is<PageToCrawl>(p => p.Uri == page1Crawled.Uri), null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.Is<PageToCrawl>(p => p.Uri == page2Crawled.Uri), null), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.Is<PageToCrawl>(p => p.Uri == page3Crawled.Uri), null), Times.Never());

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(rootCrawled, null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(page1Crawled, null));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(page2Crawled, null), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(page3Crawled, null), Times.Never());

            _fakePageRequester.Verify(f => f.MakeRequest(root.Uri));
            _fakePageRequester.Verify(f => f.MakeRequest(page1Crawled.Uri));
            _fakePageRequester.Verify(f => f.MakeRequest(page2Crawled.Uri), Times.Never());
            _fakePageRequester.Verify(f => f.MakeRequest(page3Crawled.Uri), Times.Never());

            _fakeHyperLinkParser.Verify(f => f.GetLinks(rootCrawled));
            _fakeHyperLinkParser.Verify(f => f.GetLinks(page1Crawled), Times.Never());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(page2Crawled), Times.Never());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(page3Crawled), Times.Never());

            Assert.AreEqual(2, _uut.CrawlCount);
        }

        [TestMethod]
        public void Crawl_PageRequesterThrowsException_LogsError()
        {
            //Arrange
            PageToCrawl root = new PageToCrawl(new Uri("http://a.com/"));

            CrawledPage rootCrawled = new CrawledPage(new Uri("http://a.com/"));

            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), null)).Returns(new CrawlDecision { Allow = true });
            _fakePageRequester.Setup(f => f.MakeRequest(root.Uri)).Throws(new Exception("Oh no!!!"));

            //Act
            _uut.Crawl(root);

            //Assert
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(root, null));
            _fakePageRequester.Verify(f => f.MakeRequest(root.Uri));
            _fakeLogger.Verify(f => f.Error("Oh no!!!"));
            Assert.AreEqual(1, _uut.CrawlCount);
        }
    }
}
