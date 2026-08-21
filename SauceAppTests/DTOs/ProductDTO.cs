namespace SauceAppTests.DTOs
{
    /// <summary>
    /// Strongly-typed row from SauceAppTests/TestData/products.csv (columns: Key, Name), keyed by
    /// <see cref="Key"/> (e.g. "First", "Second"). Property names match the CSV header names so
    /// CsvHelper binds them without extra configuration.
    /// </summary>
    public sealed class ProductDTO
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
