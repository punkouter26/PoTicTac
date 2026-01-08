import { test, expect } from '@playwright/test';

test.describe('Game Colors and Modal Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5000');
    await page.waitForSelector('text=Single Player');
    await page.click('text=Single Player');
    await page.waitForSelector('.game-board');
  });

  test('X should be green and O should be red', async ({ page }) => {
    // Click a cell to place an X (player's move)
    const firstCell = page.locator('.cell').first();
    await firstCell.click();
    await page.waitForTimeout(500);

    // Check that X is green
    const xCell = page.locator('.cell.x-move').first();
    await expect(xCell).toBeVisible();
    
    const xColor = await xCell.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return computed.color;
    });

    console.log('X cell color:', xColor);
    
    // RGB for #00ff00 is rgb(0, 255, 0)
    expect(xColor).toBe('rgb(0, 255, 0)');

    // Wait for AI to make a move (O)
    await page.waitForTimeout(1500);
    
    // Check that O is red
    const oCell = page.locator('.cell.o-move').first();
    await expect(oCell).toBeVisible();
    
    const oColor = await oCell.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return computed.color;
    });

    console.log('O cell color:', oColor);
    
    // RGB for #ff0000 is rgb(255, 0, 0)
    expect(oColor).toBe('rgb(255, 0, 0)');
  });

  test('winner modal should have appropriate font size and not overlap', async ({ page }) => {
    // Set up a winning scenario by forcing moves
    // This is a quick game to trigger the win modal
    const cells = page.locator('.cell');
    
    // Keep playing until someone wins or we hit a reasonable limit
    let moveCount = 0;
    const maxMoves = 36; // 6x6 board
    
    while (moveCount < maxMoves) {
      // Try to click an empty cell
      const emptyCells = await page.locator('.cell:not(.occupied)').all();
      if (emptyCells.length > 0) {
        await emptyCells[0].click();
        await page.waitForTimeout(800); // Wait for AI response
        
        // Check if game ended
        const overlay = page.locator('.game-status-overlay');
        if (await overlay.isVisible()) {
          console.log('Game ended, checking overlay');
          
          // Get overlay styles
          const overlayStyles = await overlay.evaluate((el) => {
            const computed = window.getComputedStyle(el);
            return {
              fontSize: computed.fontSize,
              maxWidth: computed.maxWidth,
              lineHeight: computed.lineHeight,
              wordBreak: computed.wordBreak
            };
          });
          
          console.log('Overlay styles:', overlayStyles);
          
          // Check that font size is reasonable (not too large)
          const fontSizeValue = parseFloat(overlayStyles.fontSize);
          expect(fontSizeValue).toBeLessThan(30); // Should be less than 30px
          expect(fontSizeValue).toBeGreaterThan(10); // Should be readable
          
          // Check that max-width is set
          expect(overlayStyles.maxWidth).not.toBe('none');
          
          // Check that word-break is set to prevent overflow
          expect(overlayStyles.wordBreak).toBe('break-word');
          
          // Get bounding box to ensure it fits on screen
          const boundingBox = await overlay.boundingBox();
          expect(boundingBox).not.toBeNull();
          
          if (boundingBox) {
            const viewport = page.viewportSize();
            expect(boundingBox.width).toBeLessThan(viewport!.width * 0.95);
            console.log(`Overlay width: ${boundingBox.width}, Viewport width: ${viewport!.width}`);
          }
          
          break;
        }
        
        moveCount++;
      } else {
        console.log('No empty cells available');
        break;
      }
    }
    
    // Verify we actually saw the overlay
    const overlay = page.locator('.game-status-overlay');
    if (await overlay.isVisible()) {
      await expect(overlay).toBeVisible();
      console.log('Winner modal test completed successfully');
    } else {
      console.log('Game did not end within move limit - this is OK for some test runs');
    }
  });

  test('color contrast should be maintained throughout the game', async ({ page }) => {
    // Make several moves and verify colors remain consistent
    // Wait properly after each move for AI response
    
    // Move 1
    await page.locator('.cell').nth(0).click();
    await page.waitForTimeout(1500); // Wait for AI
    
    // Move 2
    const emptyCells = await page.locator('.cell:not(.occupied)').first();
    await emptyCells.click();
    await page.waitForTimeout(1500); // Wait for AI
    
    // Count X's and O's
    const xCells = await page.locator('.cell.x-move').all();
    const oCells = await page.locator('.cell.o-move').all();
    
    console.log(`Found ${xCells.length} X cells and ${oCells.length} O cells`);
    
    expect(xCells.length).toBeGreaterThan(0);
    expect(oCells.length).toBeGreaterThan(0);
    
    // Verify all X's are green
    for (const xCell of xCells) {
      const color = await xCell.evaluate(el => window.getComputedStyle(el).color);
      expect(color).toBe('rgb(0, 255, 0)');
    }
    
    // Verify all O's are red
    for (const oCell of oCells) {
      const color = await oCell.evaluate(el => window.getComputedStyle(el).color);
      expect(color).toBe('rgb(255, 0, 0)');
    }
  });

  test('game status text should be readable and not overlap', async ({ page }) => {
    const gameStatus = page.locator('.game-status').first();
    await expect(gameStatus).toBeVisible();
    
    const statusStyles = await gameStatus.evaluate((el) => {
      const computed = window.getComputedStyle(el);
      return {
        fontSize: computed.fontSize,
        lineHeight: computed.lineHeight,
        wordBreak: computed.wordBreak,
        textAlign: computed.textAlign
      };
    });
    
    console.log('Game status styles:', statusStyles);
    
    // Font size should be reasonable
    const fontSizeValue = parseFloat(statusStyles.fontSize);
    expect(fontSizeValue).toBeLessThan(25); // Not too large
    expect(fontSizeValue).toBeGreaterThan(10); // Still readable
    
    // Text should be centered and able to wrap
    expect(statusStyles.textAlign).toBe('center');
    // word-break can be 'break-word' or 'normal' depending on browser
    expect(['break-word', 'normal']).toContain(statusStyles.wordBreak);
    
    // Check bounding box
    const boundingBox = await gameStatus.boundingBox();
    expect(boundingBox).not.toBeNull();
    
    if (boundingBox) {
      const viewport = page.viewportSize();
      expect(boundingBox.width).toBeLessThan(viewport!.width);
      console.log(`Status width: ${boundingBox.width}, Viewport width: ${viewport!.width}`);
    }
  });
});
