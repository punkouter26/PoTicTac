import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for PoTicTac E2E tests
 * Tests run against local development server on http://localhost:5000
 */
export default defineConfig({
  testDir: './tests',
  
  // Maximum time one test can run
  timeout: 60 * 1000,  // Increased to 60s for Blazor WASM loading
  
  // Test execution settings
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  
  // Reporter configuration
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['list'],
    ['json', { outputFile: 'test-results.json' }]
  ],
  
  // Shared settings for all tests
  use: {
    // Base URL for the application
    baseURL: 'http://localhost:5000',
    
    // Collect trace on first retry
    trace: 'on-first-retry',
    
    // Screenshot on failure and for visual regression
    screenshot: 'only-on-failure',
    
    // Video on failure
    video: 'retain-on-failure',
  },

  // Configure projects for different browsers and viewports
  projects: [
    {
      name: 'chromium-desktop',
      use: { 
        ...devices['Desktop Chrome'],
        viewport: { width: 1920, height: 1080 }
      },
    },
    {
      name: 'chromium-mobile',
      use: { 
        viewport: { width: 414, height: 896 },
        // Use desktop user agent to avoid Blazor WASM loading issues
        userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
        actionTimeout: 30000,
      },
    },
    // Accessibility testing
    {
      name: 'accessibility',
      use: { 
        ...devices['Desktop Chrome'],
      },
      testMatch: '**/*.accessibility.spec.ts',
    },
  ],

  // NOTE: For local development, manually start:
  // 1. Azurite (for Azure Table Storage emulation)
  // 2. API Server: dotnet run --project src/Po.TicTac.Api/Po.TicTac.Api.csproj
  // Then run: npx playwright test
});
