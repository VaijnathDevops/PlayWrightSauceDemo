namespace SauceAppTests.DTOs
{
    /// <summary>
    /// Strongly-typed row from SauceAppTests/TestData/customers.csv, keyed by <see cref="Key"/>
    /// (e.g. "Default"). Returned by <see cref="TestDataReader"/> in place of a raw dictionary.
    /// </summary>
    public sealed record CustomerRecord(string Key, string FirstName, string LastName, string ZipCode);
}
