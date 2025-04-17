using System;
using System.IO;
using AventStack.ExtentReports;
using NUnit.Framework;
using Reqnroll;
using OpenQA.Selenium;
using ReqnrollProject2.Drivers;
using ReqnrollProject2.Utility;

[Binding]
[Parallelizable(ParallelScope.Self)]
public sealed class Hooks : ExtentReport
{
    private readonly FeatureContext _featureContext;
    private readonly ScenarioContext _scenarioContext;

    public Hooks(FeatureContext featureContext, ScenarioContext scenarioContext)
    {
        _featureContext = featureContext;
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Config.Load(Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0", "Data"), "GlobalConfig.json"));
        var browser = Config.Settings.Browser.BrowserName;
        var timeout = Config.Settings.Timeouts.ExplicitWait;
        InitializeReport();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        string featureTitle = _featureContext.FeatureInfo.Title;
        string scenarioTitle = _scenarioContext.ScenarioInfo.Title;
        string[] tags = _scenarioContext.ScenarioInfo.Tags;

        // Create feature and scenario nodes in ExtentReport
        ExtentReport.CreateFeature(featureTitle);
        ExtentReport.CreateScenario(scenarioTitle);
        ExtentReport.AssignTags(tags);

        // Get the TestCaseId from scenario title (assumes format "Some Title - TC_123")
        string testCaseId = scenarioTitle.Contains("-") ? scenarioTitle.Split('-').Last().Trim() : null;

        // Load feature and common test data files
        string featureFile = featureTitle.Split(' ').First() + ".json";
        string featurePath = Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0", "StepDefinitions"), featureFile);
        string commonDataPath = Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0", "Data"), "commonDataFileName.json");

        // Check if the feature test data exists
        if (!File.Exists(featurePath))
        {
            ExtentReport.StepSkipped($"Test data file '{featureFile}' not found.");
            Assert.Ignore("Skipped: Feature-level JSON missing.");
        }

        // Check if common data exists
        if (!File.Exists(commonDataPath))
        {
            ExtentReport.StepSkipped("Common data file missing.");
            Assert.Ignore("Skipped: Common JSON missing.");
        }

        // Load the test data from JSON files
        try
        {
            TestDataManager.LoadTestData(featurePath, commonDataPath);
        }
        catch (Exception ex)
        {
            ExtentReport.StepSkipped($"Error loading test data: {ex.Message}");
            Assert.Ignore("Skipped: Data load failure.");
        }

        // Check if TestCaseId is valid and exists in the data
        if (string.IsNullOrWhiteSpace(testCaseId))
        {
            ExtentReport.StepSkipped("Missing TestCaseId in scenario title.");
            Assert.Ignore("Skipped: No TestCaseId.");
        }

        try
        {
            var data = TestDataManager.GetTestData(testCaseId);

            if (data.TryGetValue("Run", out var runFlag) && runFlag.Equals("No", StringComparison.OrdinalIgnoreCase))
            {
                ExtentReport.StepSkipped($"Run flag is set to 'No' for TestCaseId {testCaseId}");
                Assert.Ignore("Skipped: Run=No.");
            }

            Iteration.Data = data; // Store the test data for scenario steps
        }
        catch
        {
            ExtentReport.StepSkipped($"TestCaseId '{testCaseId}' not found in test data.");
            Assert.Ignore("Skipped: Invalid TestCaseId.");
        }

        // Launch the driver if everything is valid

        string browserFromTag = tags.FirstOrDefault(t => t.StartsWith("browser:", StringComparison.OrdinalIgnoreCase));
        string browserName = browserFromTag?.Split(':').Last().Trim() ?? Config.Settings.Browser.BrowserName;

        // Set the browser in a ThreadLocal variable (set once per scenario)
        TestContext.CurrentContext.Test.Properties.Get(browserName);

        // ... Existing logic for test data loading

        // Launch browser based on tag
        Driver.GetDriver(browserName);
    }

    [AfterStep]
    public void AfterStep()
    {
        var stepText = _scenarioContext.StepContext.StepInfo.Text;
        var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
        LogStep(stepType, stepText);

        if (_scenarioContext.TestError != null)
        {
            StepFailed(_scenarioContext.TestError.Message, Driver.GetDriver(), stepText);
        }
        else
        {
            StepPassed();
        }
    }

    [AfterScenario]
    public void AfterScenario()
    {
        Driver.CloseDriver();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        FlushReport();
    }
}