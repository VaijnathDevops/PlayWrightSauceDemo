namespace SauceAppTests.DTOs
{
    /// <summary>
    /// Strongly-typed row from SauceAppTests/TestData/customers.csv (columns: Key, FirstName,
    /// LastName, ZipCode), keyed by <see cref="Key"/> (e.g. "Default"). Property names match the
    /// CSV header names so CsvHelper binds them without extra configuration.
    /// </summary>
    public sealed class CustomerDTO
    {
        public string Key { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }
}
