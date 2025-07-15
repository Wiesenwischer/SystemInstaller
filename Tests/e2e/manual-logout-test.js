#!/usr/bin/env node

const { chromium } = require('playwright');

async function testLogoutBehavior() {
  console.log('🔄 Starting SystemInstaller Logout Test');
  console.log('=====================================');

  const browser = await chromium.launch({ headless: false });
  const context = await browser.newContext();
  const page = await context.newPage();

  try {
    // Step 1: Navigate to app
    console.log('📍 Step 1: Navigating to http://localhost:5000');
    await page.goto('http://localhost:5000');
    await page.waitForTimeout(3000);

    // Take screenshot
    await page.screenshot({ path: 'step1-navigation.png', fullPage: true });
    console.log('📸 Screenshot saved: step1-navigation.png');

    // Step 2: Check what page we land on
    const currentUrl = page.url();
    const pageContent = await page.textContent('body');
    console.log(`📍 Current URL: ${currentUrl}`);
    console.log(`📄 Page contains: ${pageContent?.substring(0, 100)}...`);

    // Step 3: Check if we need to login
    if (currentUrl.includes('localhost:8082') || pageContent?.includes('Sign In')) {
      console.log('🔐 Login required - entering credentials');
      
      try {
        await page.fill('#username', 'testuser');
        await page.fill('#password', 'password');
        await page.click('button[type="submit"]');
        await page.waitForURL('http://localhost:5000/**', { timeout: 10000 });
        console.log('✅ Login successful');
      } catch (error) {
        console.log('❌ Login failed or not needed:', error.message);
      }
    }

    await page.screenshot({ path: 'step2-after-auth.png', fullPage: true });
    console.log('📸 Screenshot saved: step2-after-auth.png');

    // Step 4: Check authentication status
    const authResponse = await page.request.get('/auth/user');
    console.log(`🔐 Auth status: ${authResponse.status()}`);

    if (authResponse.status() === 200) {
      const userInfo = await authResponse.json();
      console.log(`👤 Logged in as: ${userInfo.username}`);
    }

    // Step 5: Look for logout button
    console.log('🔍 Looking for logout button...');
    const logoutSelectors = [
      'button:has-text("Logout")',
      'button:has-text("Sign Out")',
      'a:has-text("Logout")',
      'a:has-text("Sign Out")',
      '[data-testid="logout"]'
    ];

    let logoutFound = false;
    for (const selector of logoutSelectors) {
      try {
        const element = await page.waitForSelector(selector, { timeout: 2000 });
        if (element) {
          console.log(`✅ Found logout button: ${selector}`);
          await element.click();
          logoutFound = true;
          break;
        }
      } catch {
        continue;
      }
    }

    if (!logoutFound) {
      console.log('⚠️  No logout button found, trying direct logout URL');
      await page.goto('http://localhost:5000/auth/logout');
    }

    await page.waitForTimeout(3000);
    await page.screenshot({ path: 'step3-after-logout.png', fullPage: true });
    console.log('📸 Screenshot saved: step3-after-logout.png');

    // Step 6: Check what happens after logout
    const afterLogoutUrl = page.url();
    const afterLogoutContent = await page.textContent('body');
    console.log(`📍 After logout URL: ${afterLogoutUrl}`);
    console.log(`📄 After logout content: ${afterLogoutContent?.substring(0, 100)}...`);

    // Step 7: THE CRITICAL TEST - F5 refresh
    console.log('🔄 CRITICAL TEST: Pressing F5 (refresh)...');
    await page.reload();
    await page.waitForTimeout(3000);

    await page.screenshot({ path: 'step4-after-f5.png', fullPage: true });
    console.log('📸 Screenshot saved: step4-after-f5.png');

    // Step 8: Check authentication after F5
    const afterF5Url = page.url();
    const afterF5Content = await page.textContent('body');
    const afterF5AuthResponse = await page.request.get('/auth/user');

    console.log('=====================================');
    console.log('🎯 CRITICAL RESULTS:');
    console.log(`📍 URL after F5: ${afterF5Url}`);
    console.log(`🔐 Auth status after F5: ${afterF5AuthResponse.status()}`);
    console.log(`📄 Page content after F5: ${afterF5Content?.substring(0, 100)}...`);

    if (afterF5AuthResponse.status() === 200) {
      console.log('❌ BUG CONFIRMED: User is automatically re-authenticated after logout + F5');
      console.log('   This means the Keycloak session was not properly terminated');
    } else {
      console.log('✅ LOGOUT WORKING: User remains logged out after F5');
    }

    console.log('=====================================');
    console.log('📸 Check the generated screenshots for visual confirmation');

  } catch (error) {
    console.error('❌ Test failed:', error);
  } finally {
    await browser.close();
  }
}

// Run if called directly
if (require.main === module) {
  testLogoutBehavior().catch(console.error);
}

module.exports = { testLogoutBehavior };
