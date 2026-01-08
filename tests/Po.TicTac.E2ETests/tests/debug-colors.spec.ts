import { test, expect } from '@playwright/test';

test('Debug cell classes and styles', async ({ page }) => {
  await page.goto('http://localhost:5000');
  await page.waitForSelector('text=Single Player');
  await page.click('text=Single Player');
  await page.waitForSelector('.game-board');
  
  // Click first cell (player X)
  const firstCell = page.locator('.cell').first();
  await firstCell.click();
  await page.waitForTimeout(500);
  
  // Wait for AI move (O)
  await page.waitForTimeout(1500);
  
  // Get all cell classes and colors
  const cellsData = await page.locator('.cell').evaluateAll((cells) => {
    return cells.map((cell, index) => ({
      index,
      classes: cell.className,
      text: cell.textContent,
      color: window.getComputedStyle(cell).color,
      borderColor: window.getComputedStyle(cell).borderColor
    })).filter(c => c.text !== ''); // Only occupied cells
  });
  
  console.log('=== ALL OCCUPIED CELLS ===');
  console.log(JSON.stringify(cellsData, null, 2));
  
  // Check specific classes
  const xCells = await page.locator('.cell.x-move').count();
  const oCells = await page.locator('.cell.o-move').count();
  
  console.log(`\nX cells count: ${xCells}`);
  console.log(`O cells count: ${oCells}`);
});
