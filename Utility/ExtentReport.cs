using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using OpenQA.Selenium;
using System.Collections.Concurrent;
using System.Globalization;
using AventStack.ExtentReports.Reporter.Config;

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

        private static readonly ConcurrentDictionary<string, ExtentTest> FeatureMap = new();

        //public static void InitializeReport()
        //{
        //    lock (_lock)
        //    {
        //        if (_extent != null)
        //            return; // already initialized

        //        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        //        string testResultPath = baseDir.Replace("bin\\Debug\\net8.0", "TestResults");
        //        ReportPath = Path.Combine(testResultPath, $"{timestamp}");
        //        Directory.CreateDirectory(ReportPath);

        //        var sparkReporter = new ExtentSparkReporter(Path.Combine(ReportPath, "ExtentReport.html"));



        //        sparkReporter.Config.DocumentTitle = "Automation Test Report";
        //        sparkReporter.Config.ReportName = "BDD Execution Report";
        //        sparkReporter.Config.Theme = Theme.Standard;

        //        _extent = new ExtentReports();
        //        _extent.AttachReporter(sparkReporter);

        //        Console.WriteLine("ExtentReport initialized at: " + ReportPath);
        //    }
        //}

        //public static void InitializeReport()
        //{
        //    lock (_lock)
        //    {
        //        if (_extent != null)
        //            return;

        //        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        //        string testResultPath = baseDir.Replace("bin\\Debug\\net8.0", "TestResults");
        //        ReportPath = Path.Combine(testResultPath, $"{timestamp}");
        //        Directory.CreateDirectory(ReportPath);

        //        var sparkReporter = new ExtentSparkReporter(Path.Combine(ReportPath, "ExtentReport.html"));

        //        // Add company logo
        //        //sparkReporter.Config.CSS = ".brand-logo { background: url('https://media.licdn.com/dms/image/v2/D560BAQEOV-bYHoC9Ug/company-logo_200_200/company-logo_200_200/0/1713184582351/kshema_general_insurance_limited_logo?e=2147483647&v=beta&t=JzyiQpbjea9oWw9-bLngrhS_wUHF-CGRgGSyJdp0fsU') no-repeat center; background-size: contain; }";

        //        sparkReporter.Config.CSS = @"
        //        .nav-wrapper .brand-logo {
        //            background: url('CompanyLogo.jpeg') no-repeat center;
        //            background-size: contain;
        //            display: inline-block;
        //            text-indent: -9999px;
        //            width: 200px;
        //            height: 40px;
        //        }
        //        .nav-wrapper .brand-logo svg {
        //            display: none;
        //        }";
        //        sparkReporter.Config.DocumentTitle = "Automation Test Report";
        //        sparkReporter.Config.ReportName = "BDD Execution Report";
        //        sparkReporter.Config.Theme = Theme.Standard;

        //        string sourceLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CompanyLogo.jpeg");
        //        string destLogo = Path.Combine(ReportPath, "CompanyLogo.jpeg");

        //        if (File.Exists(sourceLogo))
        //        {
        //            File.Copy(sourceLogo, destLogo, true);
        //        }

        //        _extent = new ExtentReports();
        //        _extent.AttachReporter(sparkReporter);

        //        Console.WriteLine("ExtentReport initialized at: " + ReportPath);
        //    }
        //}

        public static void InitializeReport()
        {
            lock (_lock)
            {
                if (_extent != null)
                    return;

                // Set up the report output directory
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string testResultPath = baseDir.Replace("bin\\Debug\\net8.0", "TestResults");
                ReportPath = Path.Combine(testResultPath, timestamp);
                Directory.CreateDirectory(ReportPath);

                // Set up Spark Reporter
                var sparkReporter = new ExtentSparkReporter(Path.Combine(ReportPath, "ExtentReport.html"));

                // Custom company logo CSS
                // sparkReporter.Config.CSS = @"
                //.nav-logo .logo {
                //    background-image: url('CompanyLogo.jpeg') !important;
                //    background-repeat: no-repeat !important;
                //    background-size: contain !important;
                //    background-position: center !important;
                //    width: 200px !important;
                //    height: 50px !important;
                //}";

                sparkReporter.Config.CSS = @"
                .nav-logo .logo {
                    background-image: url(data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBwgHBgkIBwgKCgkLDRYPDQwMDRsUFRAWIB0iIiAdHx8kKDQsJCYxJx8fLT0tMTU3Ojo6Iys/RD84QzQ5OjcBCgoKDQwNGg8PGjclHyU3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3Nzc3N//AABEIALwAyAMBIgACEQEDEQH/xAAcAAEAAgIDAQAAAAAAAAAAAAAABgcFCAECAwT/xABFEAABAwMBBQUDBgoKAwAAAAAAAQIDBAURBgcSITFBEyJRYXEUgZEVMjdSobMjcnR1gpKxssHCCBYzNkJiw9Hh8CRTVf/EABgBAQEBAQEAAAAAAAAAAAAAAAABAgME/8QAGxEBAQEAAwEBAAAAAAAAAAAAAAERAiExQRL/2gAMAwEAAhEDEQA/ALNAB2cwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABQABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACD0+06zvvnyRU0tbS1CTrTqsrWKiPzjC4cvUnBrXe1xtMq8f/XX70nKrJrZQA5TimUwqeWVKjgD4IvRM5ydXyMYrd5zUXwc5Ez6ZA7Ed1frG36SbTLcYKqX2pX7ns7Gu3d3Gc5cn1kJF6cf2FSf0gFXsLHxVe9NzXyYTlVxZ1lulLerZBcaBzn08zcsVyYXguFRfPgp9pD9kf0f23Cf+7OF4p+FfxJd2kau3O0bvY5Z459Cwx2A8ccePTog6oicVXnjoVAHSSWOL+0kY1PFzkTHxwd2ua9qOYqOb4o7oQwAzwyuETpkdMquE8+CAAdXyMYqI5zUyndyvNTsnFMoqKi9eaYKADlRqK5yojU5nDHMeiKx281eqEHIBw5Uaiq9UaidV4cAOQecdTBI7djmjevgj0VT048Ux16dEAAAAa1Xz6TKz87r96bKms2qXSQ7QLlJEzflZc3uY1P8AEqSZRDPJri2Yke2NjnvXDWplV8iJ09ZW3W6tY2eWKNzs9xytw3n0MdcNdUdTRLTTuZQzPbiWKoVWPTy3VRFT/vMz2j4YZKN9bDPDOkqojViejkRPVFXHPlk9PCceHC2+uPLeXJGdrurquwU1LbLXKsNTVMV0kyL3mMRcIiZ8ePwIVZdmeoNR0Md1qK2KP2hqOYtQ9z3vavJV4Lz5ku2yaUrbyymulsiWokpmKyaJnFd3OUVvj14eZDNObTL5p2kjts9NFVU8GGtZMitkYicmov8AuinlvddvPGZs2hNdWK7U7bfXMjpt5N+aKfMaJnjljufwPs/pAf2Fj/Gm/YwzGn9rVnuU8dPcqd9ukcuO0kej4/1kxj4YMPt+VFgsa5RUzLxTlyYXJnS/UXsFy1TfLNSaZ02ySOCla508kbtzO89zsueuN1OPBE48F5njetm+prNSPuE0cczI+890Eu85qeOFxn3ZLR2NUcVPoemnYxqSVUkrpHJzXDlan2N+0nCoioqKiKjkwvDmngok2JqpdkGtqysrPkK7VD6jeYrqaV7svyiZVmevDK8fBTJbUtey2J3yPZ3NSue1HzTYRexReSInjy59PVMVzotqUu0yiji7rY65zGonRO8h8epK9smvK+rrolniZcXLJEvDfY1+N34Jgm9LjOWvZ5qrVEKXKsmZG2ZMtkrpXK96eKcFVE9cHy3Oz6p2dVUFU2oWKOR2GS08m9G9eeFavl4p4kvTbXTJysMiYTCf+SnBP1TEat2n0mo7BU2t1mfEsqIrJFqEduqjsovzfUHaeWnVEeqNA3KtZ+CqoqSZk7Gr8x6RquU8l4KUzpjVd5sUlSy2TPfJVxdk1q9/dcqphyN+t095ItlVQ9tr1ZTplWPtr3qnTKNcn8T5djFFDV62ifM1HezQPmZn6yYan7wuo71WzbWVfC+41jWzTvTedHJUb0y+XHhn3nho3Xt00rUy0td21VSNa5q00rlzG9PDPLimF95sMa37UYmxbQLpHG1GoskblRPF0bFX4qqqLM8WXWVbZtabRN65zSNSkc5eySeXs4k8mN48PMwk8OpNAXeLedJSTqm+3cdvMlT05KhsjSU8VHSQ0kDEZFC1GManJqJ0K129Rs+QbbNup2iVatR3VEViqqfYhc6TU303eE1HpunuNMvZPnjXz7N6cF+1FKgrNCa+vtbJ8qyK9rXY7apqk7P9FEzj3ITDZFXU9Bs+qKytlSOnpqiRz3O6IiNXh78/FDC3Ha5dK+u9n0zamOTjudqx0sj08Ua3GPTiDM8YC57LtTWekfWROpqlI27zkpJXK9qeio3PuySXZHrisrK1LFeJ3z77FWmmkdlyKiZVqr1TCKvuPFuvdfUSLLX6dc+H/E59DKxU/SReHvyQzRFR2m0G2TRR9i2Wu7saLlGI5eXwXBPDNbKAA2yGtl8+k2q/O/8AqGyZW8my32nV0t8qLlmB1YtT2DIu987eRu9klmrLiVa00zS6ns89LKxntTWKtPMqcY39OPgvBPeU5sz1BNpfVDrfcFWKlqJOwqGPXCRSZwjseS8FNgSvtbbMabUVwfc6Or9iqpMdu18e+x64xvcOSizSVMbvfbXZUiddK+Gm3lVI0e7CqvVce/7TpJS2XUNP2j4KC4xdJFY2VE9//JGLts7TUFnoob1XvW5UcSxR1cCKqPYn1mKuFXzRUVSFz7Gr5DJmhulFIn+ZXxu+CIpLunTHbW7DZrFdqZtmxE6diunpmybyRKipheK5TOV4L4Hpr6WebQujX1SuWVYJUyvVE3N1f1cGfsGxtWVLJb9cI5I2d50NKju/5K9cYT3Ep17oddVU9thpKuOjjoUeiM7NVREdu45fikyrrtsi4bP7X59t964mJitL2WPT1ipLVDIsrYGu768N5VVVX7VUypqTpL61y0p9KVN+cX/tce+0a2zab11LWpC10M86VkO+3LZO9lyL0VN7PDwwWBQ7MPYdYsvtPct6BlQs6QrF3uOeG9nHUmeobBb9RUC0d0p+0j5tei4fGv1mqT8rrB6au+j9QUkctPT22GdW/hKeWKNHsd6Y4+qZPqu1Zo60Rq+4LaYvFOyY5y+jURXfYQG47FqlJFW2XeN8a/NbUxq1U96ZRRbtitSsqLdLtGyNvzmUzFcvxXCIOxPZ5rLW6PulfYW0ywy0U7Vkgj3eTF4KiJlPQqzYb/fGb8hf+8wt6k0xQW3TdVZbUzsY545GK93edvObu7zvEj+gNnf9U7lNXzV/tUzolia1sW4jUVUVc5XnwQZ2anZrltV+ka5/jQ/dMNjSudZbMP6w3+S7U9y9ndMjFkjfFvcWoicML4IhbNiSrG6KniVnt5/u1b/y3+RxZhHNd6VZq20MoVqPZ5Ype1Y/dVyZRFRcp4d4WdE9VPD267Fplg4tS6/h/Dcwn826Z/YbX2mCmr6aaWKO6SSorVkVEdJHupwavXijuCeKEz0no6Ky6XnsdwkjrYJ5HrJ3FajmuREx49CFXfYw5ZnPtF1a2FVykdSxct/STn8CYurMv1+ttiopKu51bI2tbljN5Fe/yanVTX3SFU6u2iW+rc1GuqLgkqonJN52cehO7JsabHUNlvVxbNG3vOhp2Km96uXiie7JkaTZa2h1dHeaS4xtpY6rtmU/YrlEzndznAu2mrIABpkHXIAAAAMePXn5j3J8ACgvHmAAAAIA9ePqAA8fMY5eQBRz6g4BAH8QAA6Y6J0AAdc4TI8cdUwvmAUDnKnAAAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/2Q==) !important;
                    background-repeat: no-repeat !important;
                    background-size: contain !important;
                    background-position: left center !important;
                    display: inline-block !important;
                    text-indent: -9999px !important;
                    width: 200px !important;
                    height: 50px !important;
                }";


                sparkReporter.Config.DocumentTitle = "Automation Test Report";
                sparkReporter.Config.ReportName = "BDD Execution Report";
                sparkReporter.Config.Theme = Theme.Standard;

                

                // Attach reporter
                _extent = new ExtentReports();
                _extent.AttachReporter(sparkReporter);

                Console.WriteLine("✅ ExtentReport initialized at: " + ReportPath);
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

        public static void LogStep(string stepType, string stepDescription)
        {
            switch (stepType.ToLower())
            {
                case "given":
                    _step.Value = _scenario.Value.CreateNode<Given>($"{stepDescription}");
                    break;
                case "when":
                    _step.Value = _scenario.Value.CreateNode<When>($"{stepDescription}");
                    break;
                case "then":
                    _step.Value = _scenario.Value.CreateNode<Then>($"{stepDescription}");
                    break;
                case "and":
                    _step.Value = _scenario.Value.CreateNode<And>($"{stepDescription}");
                    break;
                default:
                    _step.Value = _scenario.Value.CreateNode($"{stepType} {stepDescription}");
                    break;
            }
        }


        public static void StepPassed()
        {
            _step.Value?.Pass("Step Passed");
        }

        public static void StepFailed(string message, IWebDriver driver, string stepName)
        {
            string screenshotPath = CaptureScreenshot(driver, stepName);
            _step.Value?.Fail(message).AddScreenCaptureFromPath(screenshotPath);
        }

        public static string CaptureScreenshot(IWebDriver driver, string stepName)
        {
            string cleanStepName = string.Concat(stepName.Split(Path.GetInvalidFileNameChars()));
            string screenshotFile = $"{cleanStepName}.png";
            string fullPath = Path.Combine(ReportPath, screenshotFile);
            Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            File.WriteAllBytes(fullPath, screenshot.AsByteArray);
            return fullPath;
        }

        public static void FlushReport()
        {
            try
            {
                if (_extent != null)
                {
                    _extent.Flush();
                    Console.WriteLine("ExtentReport flushed.");
                }
                else
                {
                    Console.WriteLine("ExtentReport was never initialized!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FlushReport failed: " + ex.Message);
            }
        }
        public static void LogMessage(string message)
        {
            _scenario.Value?.Info(message);
        }
        public static void StepSkipped(string reason)
        {
            if (_scenario.Value != null)
            {
                _step.Value = _scenario.Value.CreateNode("Skip Reason");
                _step.Value.Skip(reason);
                _scenario.Value.Skip(reason);
            }
        }





    }
}
