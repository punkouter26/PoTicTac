import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('Debug Game Board Rendering', () => {
  test('should capture console output when loading game', async ({ page }) => {
    const consoleMessages: string[] = [];
    const errors: string[] = [];
    
    // Capture all console messages
    page.on('console', msg => {
      const text = `[${msg.type()}] ${msg.text()}`;
      consoleMessages.push(text);
      console.log(text);
    });
    
    // Capture page errors
    page.on('pageerror', error => {
      const text = `[PAGE ERROR] ${error.message}`;
      errors.push(text);
      console.log(text);
    });
    
    // Navigate to the app
    await page.goto('/', { waitUntil: 'load' });
    console.log('\n=== PAGE LOADED ===\n');
    
    // Wait for Blazor
    await waitForBlazorLoad(page);
    console.log('\n=== BLAZOR LOADED ===\n');
    
    // Wait a moment for initial render
    await page.waitForTimeout(1000);
    
    console.log('\n=== INITIAL CONSOLE MESSAGES ===');
    consoleMessages.forEach(msg => console.log(msg));
    
    console.log('\n=== CLICKING SINGLE PLAYER ===\n');
    
    // Click single player
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
    await singlePlayerButton.click();
    
    // Wait for any console output
    await page.waitForTimeout(2000);
    
    console.log('\n=== ALL CONSOLE MESSAGES AFTER CLICKING ===');
    consoleMessages.forEach(msg => console.log(msg));
    
    console.log('\n=== PAGE ERRORS ===');
    if (errors.length > 0) {
      errors.forEach(err => console.log(err));
    } else {
      console.log('No page errors');
    }
    
    // Check DOM
    console.log('\n=== DOM INSPECTION ===');
    const gameContainer = await page.locator('.game-container').count();
    console.log(`game-container count: ${gameContainer}`);
    
    const gameBoard = await page.locator('.game-board').count();
    console.log(`game-board count: ${gameBoard}`);
    
    const gameBoardContainer = await page.locator('.game-board-container').count();
    console.log(`game-board-container count: ${gameBoardContainer}`);
    
    const cells = await page.locator('.cell').count();
    console.log(`cells count: ${cells}`);
    
    const gameStatus = await page.locator('.game-status').count();
    console.log(`game-status count: ${gameStatus}`);
    
    if (gameStatus > 0) {
      const statusText = await page.locator('.game-status').textContent();
      console.log(`game-status text: "${statusText}"`);
    }
    
    // Take a screenshot for visual inspection
    await page.screenshot({ path: 'test-results/debug-screenshot.png', fullPage: true });
    console.log('\nScreenshot saved to test-results/debug-screenshot.png');
    
    // Get the HTML of the app div
    const appHtml = await page.locator('#app').innerHTML();
    console.log('\n=== APP HTML (first 500 chars) ===');
    console.log(appHtml.substring(0, 500));
  });
});
