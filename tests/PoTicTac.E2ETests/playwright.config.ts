import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for PoTicTac E2E tests
 * Tests run against local development server on http://localhost:5000
 */
export default defineConfig({
  testDir: './tests',
  
  // Maximum time one test can run
  timeout: 30 * 1000,
  
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
    
    // Screenshot on failure
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
    // Mobile testing temporarily disabled - enable when UI components have data-testid attributes
    // {
    //   name: 'chromium-mobile',
    //   use: { 
    //     ...devices['iPhone 13 Pro'],
    //     viewport: { width: 414, height: 896 }
    //   },
    // },
  ],

  // Run local dev server before starting tests
  webServer: {
    command: 'dotnet run --project ../../PoTicTacServer/PoTicTacServer.csproj',
    url: 'http://localhost:5000/api/health',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
