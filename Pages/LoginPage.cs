using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ReqnrollProject2.Drivers;

namespace ReqnrollProject2.Pages
{
    public class LoginPage
    {
        public IWebDriver driver;

        public LoginPage()
        {
            driver = Driver.GetDriver();
        }
        public void NavigateToLoginPage()
        {
            driver.Navigate().GoToUrl("https://www.google.com/");
        }
        public void MaximizeTheWindow(string data)
        {
            driver.FindElement(By.XPath("//textarea[@name='q']")).Click();
            driver.FindElement(By.XPath("//textarea[@name='q']")).SendKeys(data);
        }
        public void QuitTheDriver()
        {
            driver.FindElement(By.XPath("//textarea[@name='q']")).Click();
        }
        public void UploadZipFile()
        {
            // Click the button that opens the Windows file dialog
            driver.FindElement(By.Id("uploadTrigger")).Click();
            string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\net8.0", "TestData"), "SampleUpload.zip");
            // Full path to the zip file
            string autoItExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadFile.exe");

            Process.Start(autoItExe, $"\"{zipPath}\"");

        }
    }
}
