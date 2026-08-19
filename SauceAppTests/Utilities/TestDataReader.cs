namespace SauceAppTests.Utilities
{
    /// <summary>
    /// Minimal CSV lookup reader for simple, comma-separated, no-quoting-required TestData files
    /// (see SauceAppTests/TestData/*.csv). Rows are keyed by their first column so tests can look
    /// up a named record (e.g. "Default" customer, "First"/"Second" product) instead of hardcoding
    /// values inline.
    /// </summary>
    public static class TestDataReader
    {
        // Test host runs from SauceAppTests/bin/Debug/net10.0 - three levels up is SauceAppTests/.
        private static string TestDataDirectory =>
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

        /// <summary>
        /// Reads <paramref name="fileName"/> from SauceAppTests/TestData/ and returns the row whose
        /// first column equals <paramref name="key"/>, as a column-name -> value map (header-driven).
        /// </summary>
        public static IReadOnlyDictionary<string, string> GetRow(string fileName, string key)
        {
            var path = Path.Combine(TestDataDirectory, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"TestData file not found: {path}");
            }

            var lines = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length < 2)
            {
                throw new InvalidOperationException($"TestData file '{fileName}' has no data rows.");
            }

            var headers = lines[0].Split(',');
            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');
                if (values[0] == key)
                {
                    var row = new Dictionary<string, string>();
                    for (var i = 0; i < headers.Length && i < values.Length; i++)
                    {
                        row[headers[i]] = values[i];
                    }
                    return row;
                }
            }

            throw new KeyNotFoundException($"Key '{key}' not found in TestData file '{fileName}'.");
        }
    }
}
