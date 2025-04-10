using System;
using System.Collections.Generic;
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
    }
}
