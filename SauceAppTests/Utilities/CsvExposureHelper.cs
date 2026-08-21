using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace SauceAppTests.Utilities
{
    public static class CsvExposureHelper
    {
        public static List<T> ReadCsvToObject<T>(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            };
            using StreamReader reader = new StreamReader(filePath);
            using CsvReader csvReader = new CsvReader(reader, config);
            return new List<T>(csvReader.GetRecords<T>());
        }
    }
}
