using System.Diagnostics;
using AventStack.ExtentReports;
using NUnit.Framework;
using ReqnrollProject2.Drivers;
using ReqnrollProject2.Utility;

namespace SpecFlowBDDAutomationFramework.Hooks
{
    [Binding]
    [Parallelizable(ParallelScope.Self)]
    public sealed class Hooks : ExtentReport
    {
        public readonly FeatureContext _featureContext;
        public readonly ScenarioContext _scenarioContext;

        public Hooks(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            _featureContext = featureContext;
            _scenarioContext = scenarioContext;
        }
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            ExtentReport.InitializeReport(); // ✅ MUST be called here before anything else uses ExtentReport
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            string featureTitle = _featureContext.FeatureInfo.Title;
            string scenarioTitle = _scenarioContext.ScenarioInfo.Title;

            // ✅ Get feature file name from code-behind (matches Launch.feature = Launch.json)
            string featureFile = featureTitle.Split(' ').First() + ".json";
            string featurePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\net8.0", "StepDefinitions"), featureFile);

            string commonDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\net8.0", "Data"), "commonDataFileName.json");

            // Check if feature JSON file exists
            if (!File.Exists(featurePath))
            {
                LogEarlySkip(featureTitle, scenarioTitle, $"⏭ Skipped - JSON file '{featureFile}' not found in StepDefinitions.");
                Assert.Ignore("Skipped - Missing JSON file.");
            }

            // Check if common data file exists
            if (!File.Exists(commonDataPath))
            {
                LogEarlySkip(featureTitle, scenarioTitle, $"⏭ Skipped - Common data file 'commonDataFileName.json' not found.");
                Assert.Ignore("Skipped - Missing common data.");
            }

            // Try loading data
            try
            {
                TestDataManager.LoadTestData(featurePath, commonDataPath);
            }
            catch (Exception ex)
            {
                LogEarlySkip(featureTitle, scenarioTitle, $"⏭ Skipped - Failed to load test data: {ex.Message}");
                Assert.Ignore("Skipped - Load failure.");
            }

            // Extract TestCaseId
            if (!scenarioTitle.Contains("-"))
            {
                LogEarlySkip(featureTitle, scenarioTitle, $"⏭ Skipped - Missing TestCaseId in scenario title.");
                Assert.Ignore("Skipped - Missing TestCaseId.");
            }

            string testCaseId = scenarioTitle.Split('-').Last().Trim();

            try
            {
                var testData = TestDataManager.GetTestData(testCaseId);

                if (testData.TryGetValue("Run", out string runFlag) &&
                    runFlag.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    LogEarlySkip(featureTitle, scenarioTitle, $"⏭ Skipped - Run=No for TestCaseId: {testCaseId}");
                    Assert.Ignore("Skipped - Run=No.");
                }

                Iteration.Data = testData;
            }
            catch (KeyNotFoundException)
            {
                LogEarlySkip(featureTitle, scenarioTitle, $"⏭ Skipped - TestCaseId '{testCaseId}' not found in {featureFile}");
                Assert.Ignore("Skipped - TestCaseId not found.");
            }

            // ✅ Proceed with driver and reporting only when valid
            Driver.GetDriver();
            CreateFeature(featureTitle);
            CreateScenario(scenarioTitle);
        }


        [AfterStep]
        public void AfterStep()
        {
            var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            var stepText = _scenarioContext.StepContext.StepInfo.Text;

            LogStep(stepType, stepText);

            if (_scenarioContext.TestError == null)
            {
                StepPassed();
            }
            else
            {
                StepFailed(_scenarioContext.TestError.Message, Driver.GetDriver(), stepText);
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

        private void LogEarlySkip(string feature, string scenario, string message)
        {
            CreateFeature(feature);
            CreateScenario(scenario);
            StepSkipped(message);
        }

        private string GetStepDefinitionClassName()
        {
            // 🧠 Detect class where current method is executing and extract step class name
            var stackTrace = new StackTrace();
            foreach (var frame in stackTrace.GetFrames())
            {
                var method = frame.GetMethod();
                if (method?.DeclaringType?.Name != null &&
                    method.DeclaringType.GetCustomAttributes(typeof(BindingAttribute), false).Length > 0 &&
                    method.DeclaringType.Name.EndsWith("Steps"))
                {
                    return method.DeclaringType.Name.Replace(".cs", "").Trim();
                }
            }

            return "UnknownSteps"; // fallback if class can't be identified
        }
    }
}