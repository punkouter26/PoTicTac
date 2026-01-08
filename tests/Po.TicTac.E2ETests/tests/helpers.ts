import { Page } from '@playwright/test';

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
  } catch (e) {
    console.error('Blazor loading timeout:', e);
    // Log page content for debugging
    const content = await page.content();
    console.log('Page content:', content.substring(0, 500));
    throw e;
  }
}
