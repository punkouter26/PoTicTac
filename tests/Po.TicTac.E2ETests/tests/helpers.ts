import { Page, expect } from '@playwright/test';

/**
 * Wait for Blazor WebAssembly to finish loading
 * 
 * This helper waits for Blazor to remove the loading spinner and render the app content.
 * Critical for E2E tests as Blazor WASM can take several seconds to initialize.
 * 
 * @param page - Playwright Page object
 * @param timeout - Maximum time to wait in milliseconds (default: 60000)
 */
export async function waitForBlazorLoad(page: Page, timeout: number = 60000) {
  try {
    // Wait for Blazor to remove the loading indicator
    await page.waitForFunction(
      () => {
        const app = document.querySelector('#app');
        const loading = document.querySelector('.loading-progress');
        const errorUi = document.querySelector('#blazor-error-ui') as HTMLElement;
        
        // Check if error occurred
        if (errorUi && errorUi.style.display !== 'none') {
          console.error('Blazor error detected');
          return true;
        }
        
        // Check if loading is complete (loading indicator removed)
        return app && !loading;
      },
      { timeout }
    );
    
    // Additional stabilization wait for CSS/fonts to load
    await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => {});
    
    // Wait for fonts to be ready
    await page.evaluate(() => document.fonts.ready);
    
    // Small delay to allow CSS animations to settle
    await page.waitForTimeout(100);
  } catch (e) {
    console.error('Blazor loading timeout:', e);
    // Log page content for debugging
    const content = await page.content();
    console.log('Page content:', content.substring(0, 500));
    throw e;
  }
}

/**
 * Wait for an element to be visible with retries
 */
export async function waitForElement(page: Page, selector: string, timeout: number = 10000) {
  await expect(page.locator(selector).first()).toBeVisible({ timeout });
}

/**
 * Stabilize page for screenshot by stopping animations and waiting for fonts
 */
export async function stabilizeForScreenshot(page: Page) {
  // Disable CSS animations
  await page.addStyleTag({
    content: `
      *, *::before, *::after {
        animation-duration: 0s !important;
        animation-delay: 0s !important;
        transition-duration: 0s !important;
        transition-delay: 0s !important;
      }
    `
  });
  
  // Wait for fonts to be ready
  await page.evaluate(() => document.fonts.ready);
  
  // Wait for any pending network activity
  await page.waitForLoadState('networkidle', { timeout: 5000 }).catch(() => {});
  
  // Allow UI to stabilize
  await page.waitForTimeout(200);
}
