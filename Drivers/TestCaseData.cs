using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReqnrollProject2.Drivers
{
    public class TestCaseData
    {
        public string TestCaseId { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }

    public class TestData
    {
        public List<TestCaseData> TestCases { get; set; }
    }

    public static class TestDataManager
    {
        private static readonly object _lock = new();
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _cache = new();
        private static Dictionary<string, string> _commonData = new();

        public static void LoadTestData(string featurePath, string commonPath)
        {
            lock (_lock)
            {
                if (_cache.ContainsKey(featurePath))
                    return;

                if (!File.Exists(commonPath))
                    throw new FileNotFoundException($"Common data file not found: {commonPath}");

                var commonJson = File.ReadAllText(commonPath);
                _commonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(commonJson);

                if (!File.Exists(featurePath))
                    throw new FileNotFoundException($"Feature data file not found: {featurePath}");

                var featureJson = File.ReadAllText(featurePath);
                var featureData = JsonConvert.DeserializeObject<TestData>(featureJson);

                var dict = new Dictionary<string, Dictionary<string, string>>();

                foreach (var tc in featureData.TestCases)
                {
                    var data = new Dictionary<string, string>();
                    foreach (var kvp in tc.Data)
                    {
                        if (kvp.Value.StartsWith("#") && _commonData.TryGetValue(kvp.Value.TrimStart('#'), out var value))
                            data[kvp.Key] = value;
                        else
                            data[kvp.Key] = kvp.Value;
                    }
                    dict[tc.TestCaseId] = data;
                }

                _cache[featurePath] = dict;
            }
        }

        public static Dictionary<string, string> GetTestData(string testCaseId)
        {
            foreach (var featureData in _cache.Values)
            {
                if (featureData.TryGetValue(testCaseId, out var data))
                    return data;
            }
            throw new KeyNotFoundException($"TestCaseId '{testCaseId}' not found in loaded test data.");
        }
    }
}

