using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System;
using System.Threading;

namespace ReqnrollProject2.Drivers
{
    public static class Driver
    {
        private static ThreadLocal<IWebDriver> driver = new ThreadLocal<IWebDriver>();
        public static IWebDriver GetDriver(string browserName = null)
        {
            if (driver.Value == null)
            {
                string browser = browserName ?? Config.Settings.Browser.BrowserName;
                var time = Config.Settings.Timeouts.ImplicitWait;

                driver.Value = CreateDriver(browser);
                driver.Value.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Convert.ToDouble(Config.Settings.Timeouts.ImplicitWait));
            }
            return driver.Value;
        }
        private static IWebDriver CreateDriver(string browserName)
        {
            bool isIncognito = Config.Settings.Browser.Incognito ?? false;
            bool isHeadless = Config.Settings.Browser.Headless ?? false;

            switch (browserName.ToLowerInvariant())
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();
                    if (isIncognito) chromeOptions.AddArgument("--incognito");
                    if (isHeadless) chromeOptions.AddArgument("--headless=new");
                    chromeOptions.AddArgument("--disable-gpu");
                    chromeOptions.AddArgument("--start-maximized");
                    return new ChromeDriver(chromeOptions);

                case "edge":
                    var edgeOptions = new EdgeOptions();
                    if (isIncognito) edgeOptions.AddArgument("inprivate");
                    if (isHeadless) edgeOptions.AddArgument("headless");
                    edgeOptions.AddArgument("start-maximized");
                    return new EdgeDriver(edgeOptions);

                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    if (isIncognito) firefoxOptions.AddArgument("-private");
                    if (isHeadless) firefoxOptions.AddArgument("-headless");
                    return new FirefoxDriver(firefoxOptions);

                default:
                    throw new ArgumentException($"Unsupported browser: {browserName}");
            }
        }

        public static void CloseDriver()
        {
            if (driver.Value != null)
            {
                driver.Value.Quit();
                driver.Value.Dispose();
                driver.Value = null;
            }
        }
    }
}
