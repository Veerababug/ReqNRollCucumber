using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AventStack.ExtentReports.Gherkin.Model;
using OpenQA.Selenium;
using ReqnrollProject2.Drivers;
using ReqnrollProject2.Pages;

namespace ReqnrollProject2.StepDefinitions
{
    [Binding]
    public class Launch
    {
        public readonly LoginPage lp;

        public Launch()
        {
            lp = new LoginPage();
        }
        [Given("Open Google WebSite")]
        public void GivenOpenGoogleWebSite()
        {
            lp.NavigateToLoginPage();
        }

        [When("Maximize the window")]
        public void WhenMaximizeTheWindow()
        {
            lp.MaximizeTheWindow(Iteration.Data["Username"]);
        }

        [Then("Quit the driver")]
        public void ThenQuitTheDriver()
        {
            lp.QuitTheDriver();
        }
    }
}
