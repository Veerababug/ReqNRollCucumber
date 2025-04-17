using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;

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
                // If data is already cached, skip loading
                if (_cache.ContainsKey(featurePath))
                    return;

                // Load common data first
                LoadCommonData(commonPath);

                // Determine the file extension and load the data accordingly
                string fileExtension = Path.GetExtension(featurePath).ToLower();

                if (fileExtension == ".json")
                {
                    LoadTestDataFromJson(featurePath);
                }
                else if (fileExtension == ".csv")
                {
                    LoadTestDataFromCsv(featurePath);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported file format: {fileExtension}. Only .json and .csv are supported.");
                }
            }
        }

        private static void LoadCommonData(string commonPath)
        {
            if (!File.Exists(commonPath))
                throw new FileNotFoundException($"Common data file not found: {commonPath}");

            var commonJson = File.ReadAllText(commonPath);
            _commonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(commonJson);
        }

        private static void LoadTestDataFromJson(string featurePath)
        {
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

        private static void LoadTestDataFromCsv(string featurePath)
        {
            if (!File.Exists(featurePath))
                throw new FileNotFoundException($"Feature data file not found: {featurePath}");

            using var reader = new StreamReader(featurePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToList();

            var dict = new Dictionary<string, Dictionary<string, string>>();

            foreach (var record in records)
            {
                var data = new Dictionary<string, string>();
                var testCaseId = record.TestCaseId.ToString();

                foreach (var property in record)
                {
                    string key = property.Key;
                    string value = property.Value.ToString();

                    // Replace # with common data if applicable
                    if (value.StartsWith("#") && _commonData.TryGetValue(value.TrimStart('#'), out var commonValue))
                        data[key] = commonValue;
                    else
                        data[key] = value;
                }

                dict[testCaseId] = data;
            }

            _cache[featurePath] = dict;
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
    public static class Config
    {
        public static dynamic Settings { get; private set; }

        public static void Load(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException($"Global configuration file not found: {configFilePath}");

            var configJson = File.ReadAllText(configFilePath);
            Settings = JsonConvert.DeserializeObject<dynamic>(configJson);
        }
    }
}
