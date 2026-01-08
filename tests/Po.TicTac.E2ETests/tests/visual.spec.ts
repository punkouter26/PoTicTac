import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('Visual Regression Tests', () => {
  test.describe('Desktop Viewport', () => {
    test.use({ viewport: { width: 1920, height: 1080 } });

    test('home page should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/', { waitUntil: 'load' });
      await waitForBlazorLoad(page);
      
      // Act & Assert
      await expect(page).toHaveScreenshot('home-desktop.png', {
        fullPage: true,
        animations: 'disabled'
      });
    });

    test('game board should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/', { waitUntil: 'load' });
      await waitForBlazorLoad(page);
      await page.locator('button.mode-button:has-text("Single Player")').click();
      
      // Act - Wait for game to load
      const gameContainer = page.locator('.game-container');
      await gameContainer.waitFor({ state: 'visible', timeout: 10000 });
      
      // Assert
      await expect(gameContainer).toHaveScreenshot('game-board-desktop.png', {
        animations: 'disabled'
      });
    });

    test('statistics page should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/stats', { waitUntil: 'load' });
      await waitForBlazorLoad(page);
      
      // Act & Assert
      await expect(page).toHaveScreenshot('statistics-desktop.png', {
        fullPage: true,
        animations: 'disabled'
      });
    });
  });

  test.describe('Mobile Viewport', () => {
    test.use({ viewport: { width: 414, height: 896 } });

    test('home page mobile should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/', { waitUntil: 'load' });
      await waitForBlazorLoad(page);
      
      // Act & Assert
      await expect(page).toHaveScreenshot('home-mobile.png', {
        fullPage: true,
        animations: 'disabled'
      });
    });

    test('game board mobile should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/', { waitUntil: 'load' });
      await waitForBlazorLoad(page);
      await page.locator('button.mode-button:has-text("Single Player")').click();
      
      // Act - Wait for game to load
      const gameContainer = page.locator('.game-container');
      await gameContainer.waitFor({ state: 'visible', timeout: 10000 });
      
      // Assert
      await expect(gameContainer).toHaveScreenshot('game-board-mobile.png', {
        animations: 'disabled'
      });
    });

    test('statistics page mobile should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/stats', { waitUntil: 'load' });
      await waitForBlazorLoad(page);
      
      // Act & Assert
      await expect(page).toHaveScreenshot('statistics-mobile.png', {
        fullPage: true,
        animations: 'disabled'
      });
    });
  });

  test.describe('Responsive Layout Verification', () => {
    const viewports = [
      { name: 'desktop', width: 1920, height: 1080 },
      { name: 'laptop', width: 1366, height: 768 },
      { name: 'tablet', width: 768, height: 1024 },
      { name: 'mobile', width: 375, height: 667 }
    ];

    for (const viewport of viewports) {
      test(`home page should render correctly at ${viewport.name} (${viewport.width}x${viewport.height})`, async ({ page }) => {
        // Arrange
        await page.setViewportSize({ width: viewport.width, height: viewport.height });
        await page.goto('/', { waitUntil: 'load' });
        await waitForBlazorLoad(page);
        
        // Act & Assert
        await expect(page).toHaveScreenshot(`home-${viewport.name}-${viewport.width}x${viewport.height}.png`, {
          fullPage: true,
          animations: 'disabled'
        });
      });
    }
  });
});
