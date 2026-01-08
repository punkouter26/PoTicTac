# PoTicTac End-to-End Tests

Comprehensive E2E testing using Playwright with TypeScript, including accessibility testing with axe-core.

## Prerequisites

- Node.js 18+ installed
- .NET 9 SDK installed
- PoTicTac API server (automatically started by Playwright)

## Installation

```powershell
cd tests/PoTicTac.E2ETests
npm install
npx playwright install chromium
```

## Running Tests

### All Tests
```powershell
npm test
```

### Headed Mode (see browser)
```powershell
npm run test:headed
```

### Debug Mode
```powershell
npm run test:debug
```

### Interactive UI Mode
```powershell
npm run test:ui
```

### View Last Report
```powershell
npm run test:report
```

### Generate Tests with Codegen
```powershell
npm run test:codegen
```

## Test Structure

```
tests/
├── home.spec.ts         - Home page tests
├── gameplay.spec.ts     - Game play flow tests
├── statistics.spec.ts   - Statistics page tests
└── visual.spec.ts       - Visual regression tests (to be added)
```

## Test Coverage

### Home Page (`home.spec.ts`)
- ✅ Page loads successfully
- ✅ Game board is displayed
- ✅ No accessibility violations (WCAG 2.1 AA)
- ✅ Responsive on mobile viewport

### Gameplay (`gameplay.spec.ts`)
- ✅ Difficulty level selection
- ✅ Start new game
- ✅ Make moves on the board
- ✅ Game status display

### Statistics (`statistics.spec.ts`)
- ✅ Navigate to statistics page
- ✅ Display player statistics
- ✅ No accessibility violations (WCAG 2.1 AA)
- ✅ Leaderboard data display

## Browser & Viewport Configuration

Tests run on two configurations:

1. **Desktop (Chromium)**
   - Viewport: 1920x1080
   - Browser: Chrome

2. **Mobile (Chromium)**
   - Viewport: 414x896 (iPhone 13 Pro)
   - Browser: Chrome Mobile

## Accessibility Testing

All page tests include automatic accessibility scans using @axe-core/playwright:

```typescript
const accessibilityScanResults = await new AxeBuilder({ page })
  .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
  .analyze();

expect(accessibilityScanResults.violations).toEqual([]);
```

This ensures **WCAG 2.1 Level AA** compliance.

## Test Reports

After running tests, view the HTML report:

```powershell
npx playwright show-report
```

Reports include:
- Test execution results
- Screenshots of failures
- Videos of failed tests
- Traces for debugging

## CI/CD Integration

Tests are configured to run in CI with:
- Automatic server startup
- Retry on failure (2 retries in CI)
- HTML and JSON reports
- Screenshots and videos on failure

## Debugging

### Visual Debugging
```powershell
npm run test:debug
```

### Trace Viewer
After a test failure, open the trace:
```powershell
npx playwright show-trace trace.zip
```

### Inspector
Use Playwright Inspector for step-by-step debugging:
```powershell
PWDEBUG=1 npm test
```

## Writing New Tests

1. Create a new `.spec.ts` file in `tests/`
2. Import test utilities:
   ```typescript
   import { test, expect } from '@playwright/test';
   import AxeBuilder from '@axe-core/playwright';
   ```
3. Use descriptive test names following AAA pattern
4. Include accessibility checks for page-level tests
5. Use data-testid attributes for reliable selectors

## Best Practices

1. **Use data-testid attributes** for stable selectors
2. **Wait for networkidle** before assertions
3. **Include accessibility tests** for all pages
4. **Test on both desktop and mobile** viewports
5. **Use descriptive test names** that explain intent
6. **Add comments** for complex test logic
7. **Keep tests isolated** - each test should be independent

## Troubleshooting

### "Timeout waiting for http://localhost:5000"
- Ensure .NET 9 SDK is installed
- Check that port 5000 is not in use
- Verify PoTicTacServer project builds successfully

### "Element not found"
- Add `await page.waitForLoadState('networkidle')`
- Increase timeout: `{ timeout: 10000 }`
- Use more specific selectors or data-testid attributes

### "Accessibility violations detected"
- Review the violation details in test output
- Fix ARIA attributes, color contrast, or semantic HTML
- Re-run tests to verify fixes

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [axe-core Playwright Integration](https://github.com/dequelabs/axe-core-npm/tree/develop/packages/playwright)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
