# Test Cases: Logout

**Source requirement:** Requirement item #1 (Authentication), logout portion — "successful login, failed login with invalid credentials, and logout." Login success/failure is covered in `TestCases/Login.md`; this file covers the logout behavior only.
**Target app:** https://www.saucedemo.com

**Assumptions:**
- "Session is cleared" is described behaviorally (return to login page, protected content no longer reachable) rather than asserting a specific storage mechanism (cookie/localStorage token), since that implementation detail is unverified this session.
- Preconditions for both cases assume a prior successful login using `TestSettings.StandardUsername` / `.Password` (per `TestCases/Login.md` TC-LOGIN-001), reusing that same credential-by-role convention.

**Confirmed against the live app (2026-08-18):**
- The account menu is a hamburger button labeled "Open Menu" (top-left of the header) which opens a sidebar; the sidebar contains a "Logout" link. Both are reachable via accessible role/name locators (`button` named "Open Menu"; `link` named "Logout") — no CSS/id selectors needed.
- After clicking Logout, the app returns to the root login URL (`https://www.saucedemo.com/`) and shows the login form (username/password fields, Login button) again.
- Both re-navigating with the browser Back button and navigating directly to the inventory URL after logout produce the same confirmed behavior: the app blocks it outright. The URL briefly shows `/inventory.html`, then the app's client-side route guard redirects back to the root login URL within roughly 250ms, landing on the login form with the same error banner used for invalid-login attempts: "Epic sadface: You can only access '/inventory.html' when you are logged in." No authenticated/inventory content is ever rendered.

| ID | Title | Priority | Type | Preconditions | Steps | Test Data | Expected Result |
|----|-------|----------|------|----------------|-------|-----------|------------------|
| TC-LOGOUT-001 | Logged-in user can log out successfully | High | Positive | User is logged in (via `TestSettings.StandardUsername` / `.Password`) and on the inventory/products page | 1. Open the hamburger menu (button labeled "Open Menu") 2. Click the "Logout" link in the resulting sidebar | Standard-user credentials (`TestSettings.StandardUsername` / `.Password`) for the precondition login | User is returned to the root login page (`https://www.saucedemo.com/`); the login form (username/password fields, Login button) is shown again; the session is cleared such that reloading the previous inventory URL does not show authenticated content (instead shows the login form and the error "Epic sadface: You can only access '/inventory.html' when you are logged in.") |
| TC-LOGOUT-002 | Protected page is not accessible after logout via back navigation or direct URL | High | Negative | User has completed TC-LOGOUT-001 (logged in, then logged out, and is currently on the login page) | 1. Click the browser's Back button, OR separately, navigate directly to the inventory/products page URL used earlier in the session 2. Observe the resulting page | N/A — no form input; navigation-only test | The application does not display authenticated/inventory content. The app blocks it outright: the URL briefly shows `/inventory.html` then the client-side route guard redirects back to the root login URL within ~250ms; the login form is shown along with the error "Epic sadface: You can only access '/inventory.html' when you are logged in.", confirming the previous session is no longer valid. Behavior is identical for both Back navigation and direct URL entry. |

Next: run `/automate-tests TestCases/Logout.md` to generate the Playwright automation for these cases. (Done — see `TestProject1/Tests/LogoutTests.cs`.)
