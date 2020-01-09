using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;

namespace SeleniumUtility.Driver
{
    public class BrowserOptions
    {
        public static ChromeOptions Chrome()
        {
            var capabilities = new ChromeOptions();
            capabilities.AddUserProfilePreference("download.prompt_for_download", false);
            capabilities.AddUserProfilePreference("safebrowsing.enabled", true);
            capabilities.AddUserProfilePreference("disable-popup-blocking", true);
            capabilities.AddArgument("start-maximized");
            capabilities.AddArgument("enable-automation");
            capabilities.AddArgument("--no-sandbox");
            capabilities.AddArgument("--disable-infobars");
            capabilities.AddArgument("--disable-dev-shm-usage");
            capabilities.AddArgument("--disable-browser-side-navigation");
            capabilities.AddArgument("--disable-gpu");
            return capabilities;
        }

        public static ChromeOptions HeadlessChrome()
        {
            var capabilities = new ChromeOptions();
            //chromeoptions.AddUserProfilePreference("download.default_directory", ChromeDownloadPath ?? @"C:\SeleniumFiles\");
            capabilities.AddUserProfilePreference("download.prompt_for_download", false);
            capabilities.AddUserProfilePreference("safebrowsing.enabled", true);
            capabilities.AddUserProfilePreference("disable-popup-blocking", true);
            capabilities.AddArgument("--headless");
            capabilities.AddArgument("--no-sandbox");
            capabilities.AddArgument("--disable-infobars");
            capabilities.AddArgument("--disable-dev-shm-usage");
            capabilities.AddArgument("--disable-browser-side-navigation");
            capabilities.AddArgument("--disable-gpu");
            return capabilities;
        }

        public static InternetExplorerOptions Ie()
        {
            var capabilities = new InternetExplorerOptions
            {
                IntroduceInstabilityByIgnoringProtectedModeSettings = true,
                IgnoreZoomLevel = true,
                EnablePersistentHover = true
            };
            capabilities.AddAdditionalCapability("security.enable_java", true);
            capabilities.AddAdditionalCapability("plugin.state.java", 2);
            return capabilities;
        }

        public static AppiumOptions IPhone(string app)
        {
            var capabilities = new AppiumOptions();
            capabilities.AddAdditionalCapability(MobileCapabilityType.AutomationName, AutomationName.iOSXcuiTest);
            capabilities.AddAdditionalCapability(MobileCapabilityType.DeviceName, "iPhone X");
            capabilities.AddAdditionalCapability(MobileCapabilityType.PlatformVersion, "12.0");
            capabilities.AddAdditionalCapability(MobileCapabilityType.App, app);
            capabilities.AddAdditionalCapability(IOSMobileCapabilityType.LaunchTimeout, 60);
            return capabilities;
        }

        public static AppiumOptions Android(string app)
        {
            var capabilities = new AppiumOptions();
            capabilities.AddAdditionalCapability(MobileCapabilityType.AutomationName, AutomationName.AndroidUIAutomator2);
            capabilities.AddAdditionalCapability(MobileCapabilityType.DeviceName, "Android Emulator");
            capabilities.AddAdditionalCapability(MobileCapabilityType.App, app);
            return capabilities;
        }
    }
}
