import { test, expect } from '@playwright/test';

test.describe('SystemInstaller Logout Test (Flexible Credentials)', () => {
  
  test('Logout behavior test - use any valid credentials', async ({ page }) => {
    console.log('🔄 Starting flexible logout test...');
    
    // Step 1: Navigate to app
    console.log('📍 Step 1: Navigating to http://localhost:5000');
    await page.goto('http://localhost:5000');
    await page.waitForTimeout(3000);

    // Step 2: Check what page we land on
    const currentUrl = page.url();
    console.log(`📍 Current URL: ${currentUrl}`);

    // Step 3: If we need to login, show instructions
    if (currentUrl.includes('localhost:8082') || await page.locator('h1:has-text("Sign In")').isVisible()) {
      console.log('🔐 Login required - using admin credentials');
      
      try {
        await page.fill('#username', 'admin');
        await page.fill('#password', 'admin');
        await page.click('button[type="submit"], #kc-login');
        await page.waitForURL('http://localhost:5000/**', { timeout: 15000 });
        console.log('✅ Login successful');
      } catch (error) {
        console.log('❌ Login failed:', error.message);
        throw error;
      }
    }

    // Step 4: Verify we're authenticated
    await page.waitForTimeout(2000);
    const authResponse = await page.request.get('/auth/user');
    console.log(`🔐 Auth status: ${authResponse.status()}`);
    
    if (authResponse.status() !== 200) {
      throw new Error('User is not authenticated. Please login first.');
    }

    const userInfo = await authResponse.json();
    console.log(`👤 Logged in as: ${userInfo.username || userInfo.sub}`);
    
    // Take screenshot of authenticated state
    await page.screenshot({ path: 'test-results/authenticated.png', fullPage: true });

    // Step 5: Perform logout
    console.log('🔄 Step 5: Attempting logout...');
    
    // Try different logout methods
    let logoutSuccess = false;
    
    // Method 1: Look for logout button
    const logoutSelectors = [
      'button:has-text("Logout")',
      'button:has-text("Sign Out")', 
      'a:has-text("Logout")',
      'a:has-text("Sign Out")',
      '[data-testid="logout"]'
    ];

    for (const selector of logoutSelectors) {
      try {
        if (await page.locator(selector).isVisible()) {
          console.log(`✅ Found logout button: ${selector}`);
          await page.locator(selector).click();
          logoutSuccess = true;
          break;
        }
      } catch {
        continue;
      }
    }

    // Method 2: Direct logout URL if button not found
    if (!logoutSuccess) {
      console.log('⚠️  No logout button found, using direct logout URL');
      await page.goto('http://localhost:5000/auth/logout');
      logoutSuccess = true;
    }

    // Wait for logout to process
    await page.waitForTimeout(3000);
    
    // Take screenshot after logout
    await page.screenshot({ path: 'test-results/after-logout.png', fullPage: true });
    
    console.log('🔄 Step 6: CRITICAL TEST - F5 refresh...');
    
    // The critical test - refresh the page
    await page.reload();
    await page.waitForTimeout(3000);
    
    // Take screenshot after F5
    await page.screenshot({ path: 'test-results/after-f5-refresh.png', fullPage: true });
    
    // Check authentication status after F5
    const afterF5Auth = await page.request.get('/auth/user');
    const afterF5Url = page.url();
    
    console.log('');
    console.log('🎯 CRITICAL RESULTS:');
    console.log('=====================================');
    console.log(`📍 URL after F5: ${afterF5Url}`);
    console.log(`🔐 Auth status after F5: ${afterF5Auth.status()}`);
    
    if (afterF5Auth.status() === 200) {
      const userAfterF5 = await afterF5Auth.json();
      console.log('❌ BUG CONFIRMED: User is automatically re-authenticated after logout + F5');
      console.log(`   Re-authenticated as: ${userAfterF5.username || userAfterF5.sub}`);
      console.log('   This means the Keycloak session was not properly terminated');
      
      // Mark test as failed but don't throw - we want to see the results
      expect(afterF5Auth.status(), 'User should be logged out after F5 refresh').toBe(401);
    } else {
      console.log('✅ LOGOUT WORKING: User remains logged out after F5');
      console.log('   Keycloak session was properly terminated');
    }
    
    console.log('=====================================');
    console.log('📸 Check test-results/ folder for screenshots');
    console.log('');
  });
});
