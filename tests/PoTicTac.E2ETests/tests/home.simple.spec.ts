import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

/**
 * Simplified E2E tests for PoTicTac Home Page
 * These tests work with the current Blazor UI structure
 */
test.describe('PoTicTac Home Page - Simple Tests', () => {
  test('should load home page and display title', async ({ page }) => {
    // Arrange & Act - Navigate to homepage
    await page.goto('/');
    
    // Assert - Check for title
    await expect(page.locator('h1.game-title')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('h1.game-title')).toHaveText('PoTicTac');
  });

  test('should display game mode buttons', async ({ page }) => {
    // Arrange & Act
    await page.goto('/');
    
    // Assert - Check for mode selection buttons
    const singlePlayerButton = page.locator('button:has-text("Single Player")');
    const multiplayerButton = page.locator('button:has-text("Multiplayer")');
    
    await expect(singlePlayerButton).toBeVisible({ timeout: 10000 });
    await expect(multiplayerButton).toBeVisible();
  });

  test('should have player name input', async ({ page }) => {
    // Arrange & Act
    await page.goto('/');
    
    // Assert
    const nameInput = page.locator('input[placeholder*="Name"]');
    await expect(nameInput).toBeVisible({ timeout: 10000 });
  });

  test('should have view statistics button', async ({ page }) => {
    // Arrange & Act
    await page.goto('/');
    
    // Assert
    const statsButton = page.locator('button:has-text("View Statistics")');
    await expect(statsButton).toBeVisible({ timeout: 10000 });
  });
});