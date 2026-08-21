using SauceAppTests.DTOs;

namespace SauceAppTests.Utilities
{
    /// <summary>
    /// CSV reader for TestData files (see SauceAppTests/TestData/*.csv), backed by CsvHelper.
    /// Each file is parsed into a list of strongly-typed DTOs (<see cref="CustomerRecord"/>,
    /// <see cref="ProductRecord"/>, or any caller-supplied type) rather than raw dictionaries, so
    /// callers get compile-time-checked property access instead of magic string column lookups.
    /// </summary>
    public static class TestDataReader
    {
        // Test host runs from SauceAppTests/bin/Debug/net10.0 - three levels up is SauceAppTests/.
        private static string TestDataDirectory =>
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData");

        /// <summary>
        /// Returns every row in SauceAppTests/TestData/customers.csv as <see cref="CustomerRecord"/>s.
        /// </summary>
        public static IReadOnlyList<CustomerRecord> GetCustomers() =>
            ReadCsvToObject<CustomerRecord>("customers.csv");

        /// <summary>
        /// Returns the customer record whose Key column equals <paramref name="key"/> (e.g. "Default").
        /// </summary>
        public static CustomerRecord GetCustomer(string key) =>
            GetCustomers().FirstOrDefault(c => c.Key == key)
                ?? throw new KeyNotFoundException($"Key '{key}' not found in TestData file 'customers.csv'.");

        /// <summary>
        /// Returns every row in SauceAppTests/TestData/products.csv as <see cref="ProductRecord"/>s.
        /// </summary>
        public static IReadOnlyList<ProductRecord> GetProducts() =>
            ReadCsvToObject<ProductRecord>("products.csv");

        /// <summary>
        /// Returns the product record whose Key column equals <paramref name="key"/> (e.g. "First").
        /// </summary>
        public static ProductRecord GetProduct(string key) =>
            GetProducts().FirstOrDefault(p => p.Key == key)
                ?? throw new KeyNotFoundException($"Key '{key}' not found in TestData file 'products.csv'.");

        /// <summary>
        /// Reads <paramref name="fileName"/> from SauceAppTests/TestData/ and maps each row onto
        /// <typeparamref name="T"/> by header name (case-insensitive). Header/field-count mismatches
        /// are tolerated so TestData files can gain columns without updating every DTO.
        /// </summary>
        public static IReadOnlyList<T> ReadCsvToObject<T>(string fileName)
        {
            var path = Path.Combine(TestDataDirectory, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"TestData file not found: {path}");
            }

            return CsvExposureHelper.ReadCsvToObject<T>(path);
        }
    }
}
