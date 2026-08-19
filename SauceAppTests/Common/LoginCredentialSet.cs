namespace SauceAppTests.Common
{
    /// <summary>
    /// Named credential combinations used to drive Login_WithVariousCredentialSets_ProducesExpectedOutcome
    /// (TC-LOGIN-008) in SauceAppTests.Tests.AuthenticationTests.
    /// </summary>
    public enum LoginCredentialSet
    {
        StandardUser,
        LockedOutUser,
        StandardUserWithWrongPassword
    }
}
