using AventStack.ExtentReports.Gherkin.Model;
using ReqnrollProject2.Drivers;
using ReqnrollProject2.Pages;
using ReqnrollProject2.Utility;

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
