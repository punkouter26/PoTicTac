import { test, expect } from '@playwright/test';
import { waitForBlazorLoad } from './helpers';

test.describe('CSS Debugging', () => {
  test('should inspect computed styles of game board', async ({ page }) => {
    await page.goto('/');
    await waitForBlazorLoad(page);
    
    // Click Single Player
    const singlePlayerButton = page.locator('button.mode-button').filter({ hasText: 'Single Player' });
    await expect(singlePlayerButton).toBeVisible({ timeout: 15000 });
    await singlePlayerButton.click();
    await page.waitForTimeout(1000);
    
    // Get the game board element
    const gameBoard = page.locator('.game-board').first();
    await expect(gameBoard).toBeVisible();
    
    // Get computed styles
    const computedStyles = await gameBoard.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return {
        display: computed.display,
        gridTemplateColumns: computed.gridTemplateColumns,
        gridTemplateRows: computed.gridTemplateRows,
        width: computed.width,
        height: computed.height,
        minWidth: computed.minWidth,
        maxWidth: computed.maxWidth,
        aspectRatio: computed.aspectRatio,
        gap: computed.gap,
        padding: computed.padding,
        border: computed.border,
        backgroundColor: computed.backgroundColor,
        position: computed.position
      };
    });
    
    console.log('=== GAME BOARD COMPUTED STYLES ===');
    console.log(JSON.stringify(computedStyles, null, 2));
    
    // Get first cell styles
    const firstCell = page.locator('.cell').first();
    await expect(firstCell).toBeAttached();
    
    const cellStyles = await firstCell.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return {
        display: computed.display,
        width: computed.width,
        height: computed.height,
        minWidth: computed.minWidth,
        minHeight: computed.minHeight,
        aspectRatio: computed.aspectRatio,
        backgroundColor: computed.backgroundColor,
        border: computed.border,
        visibility: computed.visibility,
        opacity: computed.opacity
      };
    });
    
    console.log('=== CELL COMPUTED STYLES ===');
    console.log(JSON.stringify(cellStyles, null, 2));
    
    // Check bounding boxes
    const boardBox = await gameBoard.boundingBox();
    console.log('=== GAME BOARD BOUNDING BOX ===');
    console.log(JSON.stringify(boardBox, null, 2));
    
    const cellBox = await firstCell.boundingBox();
    console.log('=== FIRST CELL BOUNDING BOX ===');
    console.log(JSON.stringify(cellBox, null, 2));
  });
});
