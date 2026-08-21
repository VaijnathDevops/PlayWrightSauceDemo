namespace SauceAppTests.Common
{
    /// <summary>
    /// Single source of truth for every user-facing error/validation message asserted against across
    /// all pages (Login, Checkout). Centralized here instead of duplicated as private consts in each
    /// test fixture, so a copy change on the app only needs to be updated in one place.
    /// </summary>
    public static class ErrorMessages
    {
        public static class Login
        {
            public const string InvalidCredentials = "Epic sadface: Username and password do not match any user in this service";
            public const string LockedOut = "Epic sadface: Sorry, this user has been locked out.";
            public const string UsernameRequired = "Epic sadface: Username is required";
            public const string PasswordRequired = "Epic sadface: Password is required";
            public const string ProtectedPage = "Epic sadface: You can only access '/inventory.html' when you are logged in.";
        }

        public static class Checkout
        {
            public const string FirstNameRequired = "Error: First Name is required";
            public const string LastNameRequired = "Error: Last Name is required";
            public const string PostalCodeRequired = "Error: Postal Code is required";
        }
    }
}
