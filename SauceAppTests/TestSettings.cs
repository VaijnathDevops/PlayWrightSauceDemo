namespace SauceAppTests
{
    public static class TestSettings
    {
        static TestSettings()
        {
            // Loads .env from this directory or any parent directory it finds walking up from the
            // working directory (e.g. bin/Debug/net10.0 when running via `dotnet test`).
            // Does not overwrite variables already present in the environment, so real Azure Pipelines
            // secret variables (set as env vars on the test step) always win over a local .env file.
            DotNetEnv.Env.TraversePath().Load();
        }

        public static string StandardUsername => Require("SAUCE_STANDARD_USERNAME");
        public static string LockedOutUsername => Require("SAUCE_LOCKED_OUT_USERNAME");
        public static string Password => Require("SAUCE_PASSWORD");

        private static string Require(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Required environment variable '{variableName}' is not set. " +
                    "Set it in a local .env file (see .env.example) or as an Azure Pipelines secret variable.");
            }
            return value;
        }
    }
}
