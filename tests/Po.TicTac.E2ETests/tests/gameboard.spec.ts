import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('Game Board Rendering and Interaction', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
  });

  test('should render game board after starting single player game', async ({ page }) => {
    // Arrange - Click single player button to start game
    const singlePlayerButton = page.locator('button.mode-button:has-text("Single Player")');
    
    // Act - Start a single player game
    await singlePlayerButton.click();
    
    // Assert - Game board container should be visible
    const gameBoardContainer = page.locator('.game-board-container');
    await expect(gameBoardContainer).toBeVisible({ timeout: 10000 });
    
    // Assert - Game board grid should be visible
    const gameBoard = page.locator('.game-board.retro-glow');
    await expect(gameBoard).toBeVisible({ timeout: 10000 });
  });

  test('should render 36 cells in a 6x6 grid', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board-container').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Count the cells
    const cells = page.locator('.cell');
    
    // Assert - Should have exactly 36 cells (6x6 grid)
    await expect(cells).toHaveCount(36);
  });

  test('should have correct grid layout CSS properties', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Get computed styles of the game board
    const gameBoard = page.locator('.game-board');
    const gridTemplateColumns = await gameBoard.evaluate(el => 
      window.getComputedStyle(el).gridTemplateColumns
    );
    const gridTemplateRows = await gameBoard.evaluate(el => 
      window.getComputedStyle(el).gridTemplateRows
    );
    
    // Assert - Should have 6 columns and 6 rows
    const columnCount = gridTemplateColumns.split(' ').length;
    const rowCount = gridTemplateRows.split(' ').length;
    expect(columnCount).toBe(6);
    expect(rowCount).toBe(6);
  });

  test('should make cells clickable when empty', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Click on the first cell
    const firstCell = page.locator('.cell').first();
    await firstCell.click();
    
    // Assert - Cell should now have content (X or O)
    const cellContent = await firstCell.textContent();
    expect(cellContent).toMatch(/^[XO]$/);
  });

  test('should update cell with X when player clicks', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Click on the first empty cell
    const firstCell = page.locator('.cell').first();
    await firstCell.click();
    
    // Assert - Cell should contain 'X' (player's symbol)
    await expect(firstCell).toHaveText('X');
    
    // Assert - Cell should have the 'x-move' class
    await expect(firstCell).toHaveClass(/x-move/);
  });

  test('should apply correct styling to occupied cells', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Click on a cell
    const cell = page.locator('.cell').first();
    await cell.click();
    
    // Assert - Cell should have 'occupied' class
    await expect(cell).toHaveClass(/occupied/);
  });

  test('should show game status above the board', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    
    // Act & Assert - Game status should be visible
    const gameStatus = page.locator('.game-status');
    await expect(gameStatus).toBeVisible({ timeout: 10000 });
    
    // Assert - Status should indicate whose turn it is
    const statusText = await gameStatus.textContent();
    expect(statusText).toMatch(/(Player|AI)'s Turn/);
  });

  test('should display Back to Menu button', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act & Assert - Back to Menu button should be visible
    const backButton = page.locator('button.menu-button:has-text("Back to Menu")');
    await expect(backButton).toBeVisible();
  });

  test('should apply retro-glow effect to game board', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    
    // Act & Assert - Game board should have retro-glow class
    const gameBoard = page.locator('.game-board.retro-glow');
    await expect(gameBoard).toBeVisible({ timeout: 10000 });
  });

  test('should have proper aspect ratio for game board', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Get board dimensions
    const gameBoard = page.locator('.game-board');
    const box = await gameBoard.boundingBox();
    
    // Assert - Board should be roughly square (aspect ratio close to 1:1)
    expect(box).not.toBeNull();
    if (box) {
      const aspectRatio = box.width / box.height;
      // Allow some tolerance for borders/padding (should be between 0.9 and 1.1)
      expect(aspectRatio).toBeGreaterThan(0.9);
      expect(aspectRatio).toBeLessThan(1.1);
    }
  });

  test('should navigate back to menu when Back to Menu is clicked', async ({ page }) => {
    // Arrange - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Click Back to Menu
    await page.locator('button.menu-button:has-text("Back to Menu")').click();
    
    // Assert - Should be back at menu (game board should not be visible)
    const gameBoard = page.locator('.game-board');
    await expect(gameBoard).not.toBeVisible({ timeout: 5000 });
    
    // Assert - Menu should be visible again
    const menu = page.locator('.menu');
    await expect(menu).toBeVisible();
  });
});

test.describe('Game Board Responsive Design', () => {
  test('should render game board on mobile viewport', async ({ page }) => {
    // Arrange - Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    
    // Assert - Game board should be visible and responsive
    const gameBoard = page.locator('.game-board');
    await expect(gameBoard).toBeVisible({ timeout: 10000 });
    
    // Assert - All 36 cells should still be present
    const cells = page.locator('.cell');
    await expect(cells).toHaveCount(36);
  });

  test('should render game board on tablet viewport', async ({ page }) => {
    // Arrange - Set tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    
    // Assert - Game board should be visible
    const gameBoard = page.locator('.game-board');
    await expect(gameBoard).toBeVisible({ timeout: 10000 });
  });

  test('should render game board on desktop viewport', async ({ page }) => {
    // Arrange - Set desktop viewport
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    
    // Act - Start game
    await page.locator('button.mode-button:has-text("Single Player")').click();
    
    // Assert - Game board should be visible
    const gameBoard = page.locator('.game-board');
    await expect(gameBoard).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Game Board Accessibility', () => {
  test('should have accessible game board structure', async ({ page }) => {
    // Arrange - Start game
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Run accessibility scan on game board
    const gameBoard = page.locator('.game-board');
    
    // Assert - Game board should be in the accessibility tree
    await expect(gameBoard).toBeVisible();
    
    // Assert - Cells should have data-tooltip attributes for accessibility
    const firstCell = page.locator('.cell').first();
    const tooltip = await firstCell.getAttribute('data-tooltip');
    expect(tooltip).toBeTruthy();
    expect(tooltip).toContain('Click to place');
  });

  test('should update tooltips after cell is occupied', async ({ page }) => {
    // Arrange - Start game
    await page.goto('/', { waitUntil: 'load' });
    await waitForBlazorLoad(page);
    await page.locator('button.mode-button:has-text("Single Player")').click();
    await page.locator('.game-board').waitFor({ state: 'visible', timeout: 10000 });
    
    // Act - Click a cell
    const cell = page.locator('.cell').first();
    await cell.click();
    
    // Assert - Tooltip should now indicate the cell is occupied
    const tooltip = await cell.getAttribute('data-tooltip');
    expect(tooltip).toBeTruthy();
    expect(tooltip).toMatch(/^[XO] at/);
  });
});
