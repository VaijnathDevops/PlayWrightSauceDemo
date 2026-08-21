namespace SauceAppTests.Common
{
    /// <summary>
    /// Named customer-info field combinations used to drive
    /// Checkout_WithVariousFieldCombinations_ProducesExpectedOutcome (TC-CHECKOUT-003) in
    /// SauceAppTests.Tests.CheckoutTests.
    /// </summary>
    public enum CheckoutValidationScenario
    {
        MissingFirstName,
        MissingLastName,
        MissingPostalCode,
        AllFieldsMissing,
        AllFieldsValid
    }
}
