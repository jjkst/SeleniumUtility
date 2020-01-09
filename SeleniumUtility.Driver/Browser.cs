using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SeleniumUtility.Driver
{
    public class Browser
    {
        private RemoteWebDriver WebDriver { get; set; }
        public Browser()
        {
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            WebDriver = new ChromeDriver(path, BrowserOptions.Chrome());
            ErrorLog = new List<string>();
        }
        public Browser(IWebDriver webDriver)
        {
            WebDriver = (RemoteWebDriver)webDriver ?? throw new System.Exception("WebDriver is empty or null");
            ErrorLog = new List<string>();
        }

        public Browser(RemoteWebDriver webDriver)
        {
            WebDriver = webDriver ?? throw new System.Exception("WebDriver is empty or null");
            ErrorLog = new List<string>();
        }

        #region WaitTill

        public int WaitForElementsInSec = 3;
        public void SetImplicitlyWait(int sec)
        {
            WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sec);
        }
        public void WaitForPageLoadInSec(int sec)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(sec));
            IJavaScriptExecutor js = (IJavaScriptExecutor)WebDriver;
            try
            {
                wait.Until(
                    dr => js.ExecuteScript("return document.readyState").Equals("complete"));

            }
            catch (InvalidOperationException ex) { }
            finally
            {
                js.ExecuteScript("return window.stop");
            }
        }
        public void WaitUntil(Func<IWebDriver, System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>> condition, int maxsec)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(maxsec));
            wait.Until(condition);
        }
        public void WaitTillElementPresent(By by, int maxsec)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.PresenceOfAllElementsLocatedBy(by));
        }

        public void WaitTillElementToClick(By by, int maxsec)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }

        public void WaitTillElementToSelect(By by, int maxsec)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeSelected(by));
        }

        public void WaitTillSwitchFrame(By by, int maxsec)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.FrameToBeAvailableAndSwitchToIt(by));
        }

        #endregion

        public bool Debug { get; set; } = false;
        public List<string> ErrorLog { get; set; }
        public void Navigate(string url)
        {
            WebDriver.Navigate().GoToUrl(url);
        }

        public string GetTitle() => WebDriver.Title;

        #region Findelement, FindElements and Frame        

        public IList<IWebElement> GetElements(By by)
        {
            if (by == null) return null;
            WaitTillElementPresent(by, WaitForElementsInSec);
            IList<IWebElement> elements = WebDriver.FindElements(by);
            if (elements.Count == 0 && !Debug)
            {
                throw new NoSuchElementException("Web Element Not Found :" + by);
            }
            return elements;
        }

        public IList<IWebElement> GetElements(By by, string frame, string window)
        {
            if (!string.IsNullOrEmpty(window)) SwitchWindow(window);
            if (string.IsNullOrEmpty(frame)) return GetElements(by);
            if (frame == "Default")
            {
                WebDriver.SwitchTo().DefaultContent();
            }
            else
            {
                var mframe = frame.Split(',');
                var cframe = GetCurrentFrameId();
                if (mframe.Last() != GetCurrentFrameId())
                {
                    //just refresh
                    var currentWindowsHandle = WebDriver.CurrentWindowHandle;
                    WebDriver.SwitchTo().Window(currentWindowsHandle);

                    foreach (var mf in mframe)
                    {
                        WaitTillSwitchFrame(By.Id(mf), WaitForElementsInSec);
                        WaitTillElementPresent(By.Id(mf), WaitForElementsInSec);
                        if ((WebDriver.FindElements(By.Id(mf))).Count == 0) return null;
                        WebDriver.SwitchTo().Frame(mf);
                    }
                }

            }
            return GetElements(by);
        }

        #endregion

        #region SendKeys
        public bool Type(By by, string fieldvalue, string frame = "", string window = "")
        {
            if (string.IsNullOrEmpty(fieldvalue) || (WebDriver == null)) return false;
            var elements = GetElements(by, frame, window);
            if (elements.Count == 0) return false;
            var element = elements.First();
            if (!fieldvalue.Contains(@":\")) element.Clear();
            var msg = AcceptAlert();
            if (!string.IsNullOrEmpty(msg)) ErrorLog.Add(msg);
            if (fieldvalue.Contains("(") || fieldvalue.Contains("&") || fieldvalue.Contains("."))
            {
                ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].value ='" + fieldvalue + "';", element);
            }
            else
            {
                element.SendKeys(fieldvalue);
            }
            return true;
        }

        #endregion

        #region Handle Alert

        private IAlert GetAlertIfAny(int i)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(i));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
            var alert = WebDriver.SwitchTo().Alert();
            return alert;
        }

        public string AcceptAlert(int i = 3)
        {
            var alert = GetAlertIfAny(i);
            if (alert == null) return String.Empty;
            var alertmsg = alert.Text;
            alert.Accept();
            return alertmsg;
        }

        public string DismissAlert(int i = 3)
        {
            var alert = GetAlertIfAny(i);
            if (alert == null) return String.Empty;
            var alertmsg = alert.Text;
            alert.Dismiss();
            return alertmsg;
        }

        #endregion

        private string GetCurrentFrameId()
        {
            var jsDriver = (IJavaScriptExecutor)WebDriver;
            string frameId;
            try
            {
                frameId = (string)(jsDriver.ExecuteScript("return window.frameElement.id"));
            }
            catch (Exception e)
            {
                frameId = "Default";  //in case of DefaultContent 
            }

            return frameId;
        }
        public bool SwitchWindow(string title)
        {
            var result = false;
            var handles = WebDriver.WindowHandles;
            foreach (var handle in handles)
            {
                try
                {
                    WebDriver.SwitchTo().Window(handle);
                    if (WebDriver.Title != title) continue;
                    result = true;
                    break;
                }
                catch (NoSuchWindowException e) { }
            }
            return result;
        }
    }
}
