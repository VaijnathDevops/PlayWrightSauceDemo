namespace SauceAppTests.DTOs
{
    /// <summary>
    /// Strongly-typed row from SauceAppTests/TestData/products.csv, keyed by <see cref="Key"/>
    /// (e.g. "First", "Second"). Returned by <see cref="TestDataReader"/> in place of a raw dictionary.
    /// </summary>
    public sealed record ProductRecord(string Key, string Name);
}
