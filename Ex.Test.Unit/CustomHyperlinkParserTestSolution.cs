
using Abot.Poco;
using Ex.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ex.Test.Unit
{
    [TestClass]
    public class CustomHyperlinkParserTestSolution
    {
        CustomHyperlinkParser _uut;

        [TestInitialize]
        public void Init()
        {
            _uut = new CustomHyperlinkParser();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLinks_NullCrawledPage()
        {
            _uut.GetLinks(null);
        }

        [TestMethod]
        public void GetLinks_ValidCrawledPage_ReturnsLinks()
        {
            //Arrange
            CrawledPage page = new CrawledPage(new Uri("http://a.com"))
            {
                Content = new PageContent 
                {
                    Text = "<a href='a1.html'><a href='a2.html'><a href='a3.html'>"
                }
            };

            //Act
            IEnumerable<Uri> result = _uut.GetLinks(page);

            //Assert
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("http://a.com/a1.html", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://a.com/a2.html", result.ElementAt(1).AbsoluteUri);
            Assert.AreEqual("http://a.com/a3.html", result.ElementAt(2).AbsoluteUri);
        }

        [TestMethod]
        public void GetLinks_NoContent_ReturnsEmptyList()
        {
            //Arrange
            CrawledPage page = new CrawledPage(new Uri("http://a.com"))
            {
                Content = null
            };

            //Act
            IEnumerable<Uri> result = _uut.GetLinks(page);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
    }
}
