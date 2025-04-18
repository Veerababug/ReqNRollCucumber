using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using OpenQA.Selenium;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace ReqnrollProject2.Utility
{
    public class ExtentReport
    {
        private static readonly object _lock = new();
        private static ExtentReports _extent;
        public static string ReportPath { get; private set; }
        private static readonly AsyncLocal<ExtentTest> _feature = new();
        private static readonly AsyncLocal<ExtentTest> _scenario = new();
        private static readonly AsyncLocal<ExtentTest> _step = new();

        public static AsyncLocal<ExtentTest> StepInfo = new();
        public static AsyncLocal<ExtentTest> ScenarioInfo => _scenario;
        public static AsyncLocal<ExtentTest> FeatureInfo => _feature;

        private static readonly ConcurrentDictionary<string, ExtentTest> FeatureMap = new();


        public static void InitializeReport()
        {
            lock (_lock)
            {
                if (_extent != null) return;

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string testResultsPath = baseDir.Replace("bin\\Debug\\net8.0", "TestResults");
                ReportPath = Path.Combine(testResultsPath, timestamp);
                Directory.CreateDirectory(ReportPath);

                string htmlReport = Path.Combine(ReportPath, "ExtentReport.html");
                var spark = new ExtentSparkReporter(htmlReport);
                spark.Config.Theme = Theme.Standard;
                spark.Config.DocumentTitle = "Automation Test Report";
                spark.Config.ReportName = "BDD Execution Report";

                // Embed base64 logo
                spark.Config.CSS = @"
.nav-logo .logo {
background-image: url(data:image/png;base64,PUT_YOUR_BASE64_LOGO_HERE) !important;
background-repeat: no-repeat !important;
background-size: contain !important;
text-indent: -9999px !important;
width: 180px !important;
height: 50px !important;
display: inline-block !important;
}";

                _extent = new ExtentReports();
                _extent.AttachReporter(spark);
                _extent.AddSystemInfo("Environment", "QA");
                _extent.AddSystemInfo("Browser", "Chrome 124");
                _extent.AddSystemInfo("Author", "Your Team");
                _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
            }
        }

        public static void CreateFeature(string featureName)
        {
            if (!FeatureMap.ContainsKey(featureName))
            {
                var feature = _extent.CreateTest<Feature>(featureName);
                FeatureMap[featureName] = feature;
            }
            _feature.Value = FeatureMap[featureName];
        }

        public static void CreateScenario(string scenarioName)
        {
            _scenario.Value = _feature.Value.CreateNode<Scenario>(scenarioName);
        }

        public static void AssignTags(string[] tags)
        {
            foreach (var tag in tags)
            {
                _scenario.Value.AssignCategory(tag);
            }
        }

        public static void LogStep(ScenarioBlock stepType, string stepText)
        {
            var scenario = _scenario.Value;
            ExtentTest stepNode = null;

            switch (stepType)
            {
                case ScenarioBlock.Given:
                    stepNode = scenario.CreateNode<Given>(stepText);
                    break;
                case ScenarioBlock.When:
                    stepNode = scenario.CreateNode<When>(stepText);
                    break;
                case ScenarioBlock.Then:
                    stepNode = scenario.CreateNode<Then>(stepText);
                    break;
                default:
                    stepNode = scenario.CreateNode<And>(stepText);
                    break;
            }

            _step.Value = stepNode; // Store for later status update
        }

        public static void StepPassed(string details = "")
        {
            _step.Value?.Pass(string.IsNullOrWhiteSpace(details) ? "Step passed" : details);
        }

        public static void StepFailed(string message, IWebDriver driver, string stepName)
        {
            string screenshotPath = CaptureScreenshot(driver, stepName);
            _step.Value?.Fail(message).AddScreenCaptureFromPath(screenshotPath);
        }

        public static void StepSkipped(string reason)
        {
            _step.Value?.Skip(reason);
            //_scenario.Value?.Skip(reason);
        }

        public static void LogSkip(string featureTitle, string scenarioTitle, string message)
        {
            if (_extent == null) return;

            var feature = _extent.CreateTest<Feature>(featureTitle);
            var scenario = feature.CreateNode<Scenario>(scenarioTitle);
            scenario.Skip(message);
        }

        public static string CaptureScreenshot(IWebDriver driver, string stepName)
        {
            string cleanName = string.Concat(stepName.Split(Path.GetInvalidFileNameChars()));
            string screenshotFile = $"{cleanName}_{DateTime.Now:HHmmss}.png";
            string fullPath = Path.Combine(ReportPath, screenshotFile);

            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            File.WriteAllBytes(fullPath, ss.AsByteArray);
            return fullPath;
        }

        public static void FlushReport()
        {
            _extent?.Flush();
        }

        public static void LogInfo(string message)
        {
            _step.Value?.Info(message); // Logs under the current Gherkin step node
        }

    }

    public static class Log
    {
        private static readonly AsyncLocal<string> _logFilePath = new AsyncLocal<string>();
        private static readonly ConcurrentDictionary<string, object> _fileLocks = new();

        public static void InitLog(string scenarioName, string reportFolderPath)
        {
            var sanitizedScenarioName = string.Join("_", scenarioName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(reportFolderPath, $"{sanitizedScenarioName}_actions.txt");
            _logFilePath.Value = filePath;

            if (!_fileLocks.ContainsKey(filePath))
                _fileLocks.TryAdd(filePath, new object());

            File.WriteAllText(filePath, $"--- Action Log for Scenario: {scenarioName} ---{Environment.NewLine}");
        }

        public static void Activity(string message)
        {
            if (string.IsNullOrEmpty(_logFilePath.Value))
                return;

            string logLine = $"{DateTime.Now:HH:mm:ss} - {message}";

            // Write to text file
            lock (_fileLocks[_logFilePath.Value])
            {
                File.AppendAllText(_logFilePath.Value, logLine + Environment.NewLine);
            }

            // Log inside the current step in ExtentReport
            ExtentReport.StepInfo?.Value?.Info(message);
        }
    }
}