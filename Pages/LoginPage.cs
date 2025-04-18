using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ReqnrollProject2.Drivers;
using ReqnrollProject2.Utility;
using Activity = ReqnrollProject2.Utility.Activity;

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
            Activity.Log("User is Navigated to Google Url");
            driver.Navigate().GoToUrl("https://www.google.com/");
        }
        public void MaximizeTheWindow(string data)
        {
            Activity.Log("User is Maximizing the Google Url");
            driver.FindElement(By.XPath("//textarea[@name='q']")).Click();
            driver.FindElement(By.XPath("//textarea[@name='q']")).SendKeys(data);
        }
        public void QuitTheDriver()
        {
            Activity.Log("User is Quiting the Driver");
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
