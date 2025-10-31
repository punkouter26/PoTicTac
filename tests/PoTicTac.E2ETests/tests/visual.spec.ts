import { test, expect } from '@playwright/test';

test.describe('Visual Regression Tests', () => {
  test.describe('Desktop Viewport', () => {
    test.use({ viewport: { width: 1920, height: 1080 } });

    test('home page should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      // Act & Assert
      await expect(page).toHaveScreenshot('home-desktop.png', {
        fullPage: true,
        animations: 'disabled'
      });
    });

    test('game board should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      // Act - Locate game board
      const gameBoard = page.locator('[data-testid="game-board"], .game-board, board').first();
      await gameBoard.waitFor({ state: 'visible', timeout: 10000 });
      
      // Assert
      await expect(gameBoard).toHaveScreenshot('game-board-desktop.png', {
        animations: 'disabled'
      });
    });

    test('statistics page should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/stats');
      await page.waitForLoadState('networkidle');
      
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
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      // Act & Assert
      await expect(page).toHaveScreenshot('home-mobile.png', {
        fullPage: true,
        animations: 'disabled'
      });
    });

    test('game board mobile should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      // Act - Locate game board
      const gameBoard = page.locator('[data-testid="game-board"], .game-board, board').first();
      await gameBoard.waitFor({ state: 'visible', timeout: 10000 });
      
      // Assert
      await expect(gameBoard).toHaveScreenshot('game-board-mobile.png', {
        animations: 'disabled'
      });
    });

    test('statistics page mobile should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/stats');
      await page.waitForLoadState('networkidle');
      
      // Act & Assert
      await expect(page).toHaveScreenshot('statistics-mobile.png', {
        fullPage: true,
        animations: 'disabled'
      });
    });
  });

  test.describe('Component Level Snapshots', () => {
    test('difficulty selector should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      // Act - Find difficulty selector
      const difficultySelector = page.locator('[data-testid="difficulty-selector"], select, .difficulty').first();
      
      const selectorCount = await difficultySelector.count();
      if (selectorCount > 0) {
        await difficultySelector.waitFor({ state: 'visible', timeout: 10000 });
        
        // Assert
        await expect(difficultySelector).toHaveScreenshot('difficulty-selector.png', {
          animations: 'disabled'
        });
      }
    });

    test('empty game board should match snapshot', async ({ page }) => {
      // Arrange
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      // Look for new game button to reset board
      const newGameButton = page.locator('button:has-text("New Game"), button:has-text("Start"), [data-testid="new-game"]').first();
      const buttonCount = await newGameButton.count();
      
      if (buttonCount > 0) {
        await newGameButton.click();
        await page.waitForTimeout(500); // Wait for board to reset
      }
      
      // Act - Get the game board
      const gameBoard = page.locator('[data-testid="game-board"], .game-board, board').first();
      await gameBoard.waitFor({ state: 'visible', timeout: 10000 });
      
      // Assert
      await expect(gameBoard).toHaveScreenshot('empty-game-board.png', {
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
        await page.goto('/');
        await page.waitForLoadState('networkidle');
        
        // Act & Assert
        await expect(page).toHaveScreenshot(`home-${viewport.name}-${viewport.width}x${viewport.height}.png`, {
          fullPage: true,
          animations: 'disabled'
        });
      });
    }
  });
});
