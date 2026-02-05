import { Page, expect } from '@playwright/test';

/**
 * Wait for React app to finish loading
 * 
 * This helper waits for the React app to render content.
 * 
 * @param page - Playwright Page object
 * @param timeout - Maximum time to wait in milliseconds (default: 30000)
 */
export async function waitForReactLoad(page: Page, timeout: number = 30000) {
  try {
    // Wait for the root element to have content
    await page.waitForFunction(
      () => {
        const root = document.querySelector('#root');
        return root && root.children.length > 0;
      },
      { timeout }
    );
    
    // Wait for network to settle
    await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => {});
    
    // Wait for fonts to be ready
    await page.evaluate(() => document.fonts.ready);
    
    // Small delay to allow React state to settle
    await page.waitForTimeout(100);
  } catch (e) {
    console.error('React loading timeout:', e);
    const content = await page.content();
    console.log('Page content:', content.substring(0, 500));
    throw e;
  }
}

/**
 * Wait for Blazor WebAssembly to finish loading (legacy - kept for compatibility)
 * @deprecated Use waitForReactLoad instead
 */
export async function waitForBlazorLoad(page: Page, timeout: number = 60000) {
  return waitForReactLoad(page, timeout);
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
