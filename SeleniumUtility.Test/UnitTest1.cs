using OpenQA.Selenium.Chrome;
using SeleniumUtility.Driver;
using System;
using System.IO;
using Xunit;

namespace SeleniumUtility.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var driver = new Browser(new ChromeDriver(path, BrowserOptions.Chrome()));
            driver.Navigate("http://www.google.com");
        }
    }
}
