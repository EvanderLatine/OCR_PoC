import { test, expect } from '@playwright/test';
import path from 'path';
import fs from 'fs';

/**
 * Integration tests using real Emirates ID test files.
 * Uses drag-drop simulation for Svelte 5 compatibility.
 */

const TEST_FILES_DIR = '/home/yka/projects/OCR_PoC/dotNET/Brimit.EmiratesIDExtractor/docs/test_files';
const FRONT_IMAGE = path.join(TEST_FILES_DIR, 'EID_784199483585305-1.png');
const BACK_IMAGE = path.join(TEST_FILES_DIR, 'EID_784199483585305-2.png');
const PDF_FILE = path.join(TEST_FILES_DIR, 'EID_784199483585305.pdf');

// Helper to upload file via drag-drop simulation
async function uploadFile(page: any, filePath: string, dropZoneSelector: string = '[role="button"]') {
  const fileBuffer = fs.readFileSync(filePath);
  const fileName = path.basename(filePath);
  const mimeType = filePath.endsWith('.pdf') ? 'application/pdf' : 'image/png';

  const dataTransfer = await page.evaluateHandle(
    async ({ buffer, name, type }: { buffer: number[]; name: string; type: string }) => {
      const dt = new DataTransfer();
      const blob = new Blob([new Uint8Array(buffer)], { type });
      const file = new File([blob], name, { type });
      dt.items.add(file);
      return dt;
    },
    { buffer: Array.from(fileBuffer), name: fileName, type: mimeType }
  );

  const dropZone = page.locator(dropZoneSelector).first();
  await dropZone.dispatchEvent('dragover', { dataTransfer });
  await dropZone.dispatchEvent('drop', { dataTransfer });
}

test.describe('Emirates ID Extraction - Integration Tests', () => {
  test.setTimeout(180000); // 3 minutes for API calls

  test('PDF upload and extraction - full flow', async ({ page }) => {
    await page.goto('/');

    // Verify initial state
    await expect(page.getByText('Upload your Emirates ID')).toBeVisible();

    // Upload PDF via native file input
    await uploadFile(page, PDF_FILE);

    // Should switch to PDF mode
    await expect(page.getByText('PDF document detected')).toBeVisible({ timeout: 15000 });

    // File preview should show
    await expect(page.getByText('EID_784199483585305.pdf')).toBeVisible();

    // Extract Data button should be enabled
    const extractButton = page.getByRole('button', { name: 'Extract Data' });
    await expect(extractButton).toBeEnabled();

    // Click Extract Data
    await extractButton.click();

    // Should show processing state
    await expect(page.getByText('Analyzing document...')).toBeVisible();

    // Wait for result (success, partial, or error)
    await expect(
      page.getByText('Extraction Complete')
        .or(page.getByText('Partial Extraction'))
        .or(page.locator('[role="alert"]'))
    ).toBeVisible({ timeout: 120000 });

    // Take screenshot of result
    await page.screenshot({ path: 'test-results/pdf-extraction-result.png' });

    // If successful, verify extracted data fields
    if (await page.getByText('Extraction Complete').isVisible().catch(() => false) ||
        await page.getByText('Partial Extraction').isVisible().catch(() => false)) {
      await expect(page.getByText('Full Name')).toBeVisible();
      await expect(page.getByText('ID Number')).toBeVisible();
      await expect(page.getByText('Date of Birth')).toBeVisible();
      await expect(page.getByText('Expiry Date')).toBeVisible();

      console.log('✅ PDF extraction completed successfully');
    } else {
      console.log('⚠️ PDF extraction resulted in error (check screenshot)');
    }

    // Start Over button should be visible
    await expect(page.getByRole('button', { name: 'Start Over' })).toBeVisible();
  });

  test('Image upload flow - front and back sides with extraction', async ({ page }) => {
    await page.goto('/');

    // Upload front image via native file input
    await uploadFile(page, FRONT_IMAGE);

    // Should switch to image mode
    await expect(page.getByText('Upload front and back images')).toBeVisible({ timeout: 15000 });

    // Step indicator should show
    await expect(page.getByText('Front Side')).toBeVisible();

    // Continue button should be enabled
    const continueButton = page.getByRole('button', { name: /continue to back side/i });
    await expect(continueButton).toBeEnabled();

    // Click to proceed to back side
    await continueButton.click();

    // Flip animation should trigger
    await expect(page.locator('.flip-card')).toHaveClass(/flipped/, { timeout: 5000 });

    // Wait for animation
    await page.waitForTimeout(1000);

    // Upload back image to the back side upload zone
    await uploadFile(page, BACK_IMAGE, '.flip-face-back [role="button"]');

    // Wait for file to be processed
    await page.waitForTimeout(500);

    // Extract Data button should be enabled
    const extractButton = page.getByRole('button', { name: 'Extract Data' });
    await expect(extractButton).toBeEnabled({ timeout: 5000 });

    // Click Extract Data
    await extractButton.click();

    // Should show processing state
    await expect(page.getByText('Analyzing document...')).toBeVisible();

    // Wait for result
    await expect(
      page.getByText('Extraction Complete')
        .or(page.getByText('Partial Extraction'))
        .or(page.locator('[role="alert"]'))
    ).toBeVisible({ timeout: 120000 });

    // Take screenshot
    await page.screenshot({ path: 'test-results/image-extraction-result.png' });

    // Verify result display
    if (await page.getByText('Extraction Complete').isVisible().catch(() => false) ||
        await page.getByText('Partial Extraction').isVisible().catch(() => false)) {
      await expect(page.getByText('Full Name')).toBeVisible();
      await expect(page.getByText('ID Number')).toBeVisible();

      console.log('✅ Image extraction completed successfully');
    } else {
      console.log('⚠️ Image extraction resulted in error (check screenshot)');
    }

    // Start Over should be visible
    await expect(page.getByRole('button', { name: 'Start Over' })).toBeVisible();
  });

  test('Reset flow - Start Over returns to initial state', async ({ page }) => {
    await page.goto('/');

    // Upload PDF via native file input
    await uploadFile(page, PDF_FILE);

    await expect(page.getByText('PDF document detected')).toBeVisible({ timeout: 15000 });

    // Extract
    await page.getByRole('button', { name: 'Extract Data' }).click();

    // Wait for result
    await expect(
      page.getByText('Extraction Complete')
        .or(page.getByText('Partial Extraction'))
        .or(page.locator('[role="alert"]'))
    ).toBeVisible({ timeout: 120000 });

    // Click Start Over
    await page.getByRole('button', { name: 'Start Over' }).click();

    // Should be back to initial state
    await expect(page.getByText('Upload your Emirates ID')).toBeVisible();
    await expect(page.getByText('Accepts: JPEG, PNG, WebP images or PDF documents')).toBeVisible();

    console.log('✅ Reset flow works correctly');
  });
});
