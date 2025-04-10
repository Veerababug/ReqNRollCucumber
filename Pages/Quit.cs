using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ReqnrollProject2.Drivers;

namespace ReqnrollProject2.Pages
{
    public class Quit
    {
        public IWebDriver driver;

        public Quit()
        {
            driver = Driver.GetDriver();
        }
        public void NavigateToLoginPage()
        {
            driver.Navigate().GoToUrl("https://www.google.com/");
        }
        public void MinimizeTheWindow()
        {
            driver.FindElement(By.XPath("//textarea[@name='qr']")).Click();
            //driver.FindElement(By.XPath("//textarea[@name='q']")).SendKeys(Iteration.Data["CompanyName"]);
        }
        public void QuitTheDriver()
        {
            driver.FindElement(By.XPath("//textarea[@name='qr']")).Click();
        }
    }
}
