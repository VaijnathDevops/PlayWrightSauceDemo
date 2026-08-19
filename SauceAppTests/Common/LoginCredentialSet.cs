namespace TestProject1.Common
{
    /// <summary>
    /// Named credential combinations used to drive Login_WithVariousCredentialSets_ProducesExpectedOutcome
    /// (TC-LOGIN-008) in TestProject1.Tests.AuthenticationTests.
    /// </summary>
    public enum LoginCredentialSet
    {
        StandardUser,
        LockedOutUser,
        StandardUserWithWrongPassword
    }
}
