using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ReqnrollProject2.Drivers;
using ReqnrollProject2.Pages;

namespace ReqnrollProject2.StepDefinitions
{
    [Binding]
    public class QuitSteps
    {
        public readonly Quit q;

        public QuitSteps()
        {
            q = new Quit();
        }
        [When("Minimize the window")]
        public void WhenMinimizeTheWindow()
        {
            q.MinimizeTheWindow();
        }
        [When("Minimize the window in Google")]
        public void WhenMinimizeTheWindowInGoogle()
        {
            q.MinimizeTheWindow();
        }

    }
}
