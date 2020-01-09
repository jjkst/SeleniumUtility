using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace SeleniumUtility.Driver
{
    public class Browser
    {
        private RemoteWebDriver webDriver;

        public RemoteWebDriver GetWebDriver()
        {
            return webDriver;
        }

        public void SetWebDriver(RemoteWebDriver value)
        {
            webDriver = value;
        }

        public Browser()
        {
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            SetWebDriver(new ChromeDriver(path, BrowserOptions.Chrome()));
            ErrorLog = new List<string>();
        }
        public Browser(IWebDriver webDriver)
        {
            SetWebDriver((RemoteWebDriver)webDriver ?? throw new WebDriverException("WebDriver is empty or null"));
            ErrorLog = new List<string>();
        }

        public Browser(RemoteWebDriver webDriver)
        {
            SetWebDriver(webDriver ?? throw new WebDriverException("WebDriver is empty or null"));
            ErrorLog = new List<string>();
        }

        #region WaitTill

        public int WaitForElementsInSec = 3;
        public void SetImplicitlyWait(int sec)
        {
            GetWebDriver().Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(sec);
        }
        public void WaitForPageLoadInSec(int sec)
        {
            var wait = new WebDriverWait(GetWebDriver(), TimeSpan.FromSeconds(sec));
            IJavaScriptExecutor js = (IJavaScriptExecutor)GetWebDriver();
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
            var wait = new WebDriverWait(GetWebDriver(), TimeSpan.FromSeconds(maxsec));
            wait.Until(condition);
        }
        public void WaitTillElementPresent(By by, int maxsec)
        {
            var wait = new WebDriverWait(GetWebDriver(), TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.PresenceOfAllElementsLocatedBy(by));
        }

        public void WaitTillElementToClick(By by, int maxsec)
        {
            var wait = new WebDriverWait(GetWebDriver(), TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }

        public void WaitTillElementToSelect(By by, int maxsec)
        {
            var wait = new WebDriverWait(GetWebDriver(), TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeSelected(by));
        }

        public void WaitTillSwitchFrame(By by, int maxsec)
        {
            var wait = new WebDriverWait(GetWebDriver(), TimeSpan.FromSeconds(maxsec));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.FrameToBeAvailableAndSwitchToIt(by));
        }

        #endregion

        public bool Debug { get; set; } = false;
        public List<string> ErrorLog { get; set; }
        public void Navigate(string url)
        {
            GetWebDriver().Navigate().GoToUrl(url);
        }

        public string GetTitle() => GetWebDriver().Title;

        #region Findelement, FindElements and Frame        

        public IList<IWebElement> GetElements(By by)
        {
            if (webDriver == null) throw new WebDriverException("WebDriver is empty or null");
            if (by == null) throw new WebDriverException("By is empty or null");
            WaitTillElementPresent(by, WaitForElementsInSec);
            IList<IWebElement> elements = GetWebDriver().FindElements(by);
            if (elements.Count == 0)
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
                GetWebDriver().SwitchTo().DefaultContent();
            }
            else
            {
                var mframe = frame.Split(',');
                if (mframe.Last() != GetCurrentFrameId())
                {
                    //just refresh
                    var currentWindowsHandle = GetWebDriver().CurrentWindowHandle;
                    GetWebDriver().SwitchTo().Window(currentWindowsHandle);

                    foreach (var mf in mframe)
                    {
                        WaitTillSwitchFrame(By.Id(mf), WaitForElementsInSec);
                        if ((GetWebDriver().FindElements(By.Id(mf))).Count == 0) throw new WebDriverException($"Given frame {frame} not exist");
                        GetWebDriver().SwitchTo().Frame(mf);
                    }
                }

            }
            return GetElements(by);
        }

        #endregion

        #region GetTextAndValues, SelectedTextAndValues, Table

        public string GetText(By by, string frame = "", string window = "")
        {
            var val = string.Empty;
            var elements = GetElements(by, frame, window);
            return elements == null ? val : elements.Aggregate(val, (current, element) => current + element.Text);
        }

        public DataTable GetTable(By by, string frame = "", string window = "")
        {
            var tableId = GetElements(by, frame, window).First();
            var th = tableId.FindElements(By.CssSelector("th"));
            var tr = tableId.FindElements(By.CssSelector("tbody > tr"));
            if (tableId.TagName == "tbody") tr = tableId.FindElements(By.CssSelector("tr"));
            var dt = new DataTable();

            if (th.Count == 0)
            {
                for (var i = 0; i < 12; i++)
                {
                    dt.Columns.Add("Column" + i);
                }
            }

            var colNum = 1;
            foreach (var header in th)
            {
                var colName = header.Text;
                if (dt.Columns.Contains(colName))
                {
                    colName += colNum;
                    colNum++;
                }
                dt.Columns.Add(colName);
            }

            foreach (var row in tr)
            {
                var dataRow = dt.NewRow();
                var i = 0;
                foreach (var td in row.FindElements(By.CssSelector("td")))
                {
                    dataRow[i] = td.Text;
                    i++;
                }
                dt.Rows.Add(dataRow);
            }

            return dt;
        }

        #endregion

        #region Actions
        public void Type(By by, string fieldvalue, string frame = "", string window = "")
        {
            if (string.IsNullOrEmpty(fieldvalue)) throw new ArgumentException("Value is empty");
            var element = GetElements(by, frame, window).First();
            if (!fieldvalue.Contains(@":\")) element.Clear();
            try
            {
                var msg = AcceptAlert();
                if (!string.IsNullOrEmpty(msg)) ErrorLog.Add(msg);
            }
            catch { }

            if (fieldvalue.Contains("(") || fieldvalue.Contains("&") || fieldvalue.Contains("."))
            {
                ((IJavaScriptExecutor)GetWebDriver()).ExecuteScript("arguments[0].value ='" + fieldvalue + "';", element);
            }
            else
            {
                WaitTillElementPresent(by, WaitForElementsInSec);
                element.SendKeys(fieldvalue);
            }
        }

        public void Click(By by, string frame = "", string window = "")
        {
            var element = GetElements(by, frame, window).First();
            if (element.Enabled && !element.Selected)
            {
                WaitTillElementToClick(by, WaitForElementsInSec);
                element.Click();
            }
        }

        #endregion

        #region Handle Alert

        private IAlert GetAlertIfAny(int i)
        {
            var wait = new WebDriverWait(GetWebDriver(), TimeSpan.FromSeconds(i));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
            var alert = GetWebDriver().SwitchTo().Alert();
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

        public string GetCurrentFrameId()
        {
            var jsDriver = (IJavaScriptExecutor)GetWebDriver();
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
            var handles = GetWebDriver().WindowHandles;
            foreach (var handle in handles)
            {
                try
                {
                    GetWebDriver().SwitchTo().Window(handle);
                    if (GetWebDriver().Title != title) continue;
                    result = true;
                    break;
                }
                catch (NoSuchWindowException e) { }
            }
            return result;
        }

        public void Close()
        {
            if(GetWebDriver() != null)
                GetWebDriver().Quit();
        }
    }
}
