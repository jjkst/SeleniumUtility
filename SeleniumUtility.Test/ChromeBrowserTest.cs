using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumUtility.Driver;
using System;
using System.IO;
using Xunit;

namespace SeleniumUtility.Test
{
    public class ChromeBrowserTest :IDisposable
    {

        private readonly Browser Chrome;
        private readonly string testUrl = "http://the-internet.herokuapp.com/";
        private readonly string testUrlTitle = "The Internet";

        public ChromeBrowserTest()
        {
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Chrome = new Browser(new ChromeDriver(path, BrowserOptions.Chrome()));
        }

        [Fact]
        [Trait("Category", "Chrome")]
        public void DriverInstance()
        {
            Assert.True(Chrome != null);
            Chrome.Navigate(testUrl);
            Assert.True(Chrome.GetTitle() == testUrlTitle);
        }

        [Fact]
        [Trait("Category", "Chrome")]
        public void Type()
        {            
            Chrome.Navigate(testUrl);
            Chrome.Click(By.LinkText("Basic Auth"));
            var f = Chrome.GetWebDriver().FindElementsByTagName("input");
        }
        
        public void Dispose()
        {
            Chrome.Close();
        }

    }
}
