# Test Coverage Report

Tracks the traceability between manual test case IDs and automated test methods. Update whenever new test cases or automation is added.

---

## Coverage Matrix

| TC ID | Manual Test Case | Automated? | Test Method | File |
|---|---|---|---|---|
| TC-LOGIN-001 | TestCases/Login.md | ✅ | Login_WithStandardUser_RedirectsToInventoryPage | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGIN-002 | TestCases/Login.md | ✅ | Login_WithIncorrectPassword_ShowsInvalidCredentialsError | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGIN-003 | TestCases/Login.md | ✅ | Login_WithUnknownUsername_ShowsInvalidCredentialsError | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGIN-004 | TestCases/Login.md | ✅ | Login_WithLockedOutUser_ShowsLockedOutError | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGIN-005 | TestCases/Login.md | ✅ | Login_WithEmptyUsername_ShowsUsernameRequiredError | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGIN-006 | TestCases/Login.md | ✅ | Login_WithEmptyPassword_ShowsPasswordRequiredError | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGIN-007 | TestCases/Login.md | ✅ | Login_WithEmptyUsernameAndPassword_ShowsUsernameRequiredError | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGIN-008 | TestCases/Login.md | ✅ | Login_WithVariousCredentialSets_ProducesExpectedOutcome [TestCase×N] | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGOUT-001 | TestCases/Logout.md | ✅ | Logout_FromInventoryPage_ReturnsToLoginPage | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-LOGOUT-002 | TestCases/Logout.md | ✅ | Logout_ThenAttemptReaccess_ProtectedInventoryPageNotAccessible [TestCase×N] | SauceAppTests/Tests/AuthenticationTests.cs |
| TC-CHECKOUT-001 | TestCases/Checkout.md | ✅ | Checkout_FullPurchaseJourney_CompletesOrderAndEmptiesCart | SauceAppTests/Tests/CheckoutTests.cs |
| TC-CHECKOUT-002 | TestCases/Checkout.md | ✅ | Checkout_WithEmptyCart_DoesNotProceedToCustomerInfoStep | SauceAppTests/Tests/CheckoutTests.cs |
| TC-CHECKOUT-003 | TestCases/Checkout.md | ✅ | Checkout_CustomerInfoValidation_RequiresFirstLastZip [TestCase×N] | SauceAppTests/Tests/CheckoutTests.cs |
| TC-CHECKOUT-004 | TestCases/Checkout.md | ✅ | ContinueShopping_FromCartPage_ReturnsToInventoryPage | SauceAppTests/Tests/CheckoutTests.cs |
| TC-CHECKOUT-005 | TestCases/Checkout.md | ✅ | Cancel_FromCustomerInfoStep_ReturnsToCartPage | SauceAppTests/Tests/CheckoutTests.cs |
| TC-NFR-001 | TestCases/NonFunctional.md | ✅ | InventoryPage_VisualAppearance_MatchesBaseline | SauceAppTests/Tests/NonFunctionalTests.cs |
| TC-NFR-002 | TestCases/NonFunctional.md | ✅ | GoogleFontsStylesheetRequest_OnPageLoad_ReturnsValidCssResponse | SauceAppTests/Tests/NonFunctionalTests.cs |
| TC-NFR-003 | TestCases/NonFunctional.md | ✅ | LoginAndInventory_WithMockedGoogleFontsFailure_StillRenderAndFunction | SauceAppTests/Tests/NonFunctionalTests.cs |
| TC-NFR-004 | TestCases/NonFunctional.md | ✅ | LoginPage_AccessibilityScan_HasNoCriticalOrSeriousViolations, InventoryPage_AccessibilityScan_HasNoCriticalOrSeriousViolations | SauceAppTests/Tests/NonFunctionalTests.cs |

> Re-synced 2026-08-21 from the audit commands below (prior matrix was a stale placeholder). Use `/write-test-cases` and `/automate-tests` commands to generate new coverage.

---

## Summary

| Metric | Count |
|---|---|
| Total manual TCs | 18 |
| Automated | 18 |
| Not automated | 0 |
| **Automation coverage** | **100%** |

---

## How to Audit Coverage

```powershell
# All manual TC IDs defined
Select-String -Path TestCases\*.md -Pattern "TC-[A-Z]+-\d{3}" | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value | Sort-Object -Unique

# All automated TC IDs referenced
Select-String -Path SauceAppTests\Tests\*.cs -Pattern "TC-[A-Z]+-\d{3}" | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value | Sort-Object -Unique
```

IDs in the first list but not the second = **automation gap**.

---

## Coverage Goals

| Phase | Target |
|---|---|
| MVP | Core happy-path flows (Login, Inventory, Checkout) |
| Phase 2 | All negative / error cases |
| Phase 3 | Edge cases, boundary values, multi-user scenarios |
