# SauceDemo — App Under Test Reference

## Test Users

| Username | Password | Behaviour |
|---|---|---|
| `standard_user` | `secret_sauce` | Normal, fully functional user |
| `locked_out_user` | `secret_sauce` | Blocked at login — sees error message |
| `problem_user` | `secret_sauce` | UI rendering issues (wrong product images, broken sort) |
| `performance_glitch_user` | `secret_sauce` | Simulated slow login (~5 s delay) |
| `error_user` | `secret_sauce` | Certain actions throw errors |
| `visual_user` | `secret_sauce` | Visual-only defects (layout, colours) |

> Use `SauceDemoConstants.Password` — never hardcode `secret_sauce` in tests.

## Error Messages

| Scenario | Message |
|---|---|
| Empty username | `Epic sadface: Username is required` |
| Empty password | `Epic sadface: Password is required` |
| Locked-out user | `Epic sadface: Sorry, this user has been locked out.` |
| Wrong credentials | `Epic sadface: Username and password do not match any user in this service` |
| Empty first name at checkout | `Error: First Name is required` |
| Empty last name at checkout | `Error: Last Name is required` |
| Empty postal code at checkout | `Error: Postal Code is required` |

## Notes

- The app is client-side only — no real backend. State resets on full page reload or new browser context.
- `PageTest` (Playwright NUnit base class) creates a fresh browser context per test by default — no explicit logout needed between tests.
- `performance_glitch_user` login can exceed default Playwright navigation timeout on slow machines — consider increasing `NavigationTimeout` for tests using that user.
