import { test, expect, Page, BrowserContext } from '@playwright/test';

class LogoutTestPage {
  constructor(private page: Page) {}

  async navigateToApp() {
    await this.page.goto('/');
  }

  async waitForLoginPage() {
    // Wait for either Keycloak login page or the "Welcome to System Installer" message
    try {
      await this.page.waitForSelector('h1:has-text("Sign In")', { timeout: 10000 });
      return 'keycloak-login';
    } catch {
      try {
        await this.page.waitForSelector('text=Welcome to System Installer', { timeout: 5000 });
        return 'welcome-page';
      } catch {
        throw new Error('Neither login page nor welcome page found');
      }
    }
  }

  async waitForDashboard() {
    // Wait for the authenticated dashboard/home page
    await this.page.waitForSelector('[data-testid="dashboard"], .dashboard, h1:has-text("Dashboard"), h1:has-text("Home")', { timeout: 10000 });
  }

  async login(username: string, password: string) {
    // Fill login form on Keycloak
    try {
      await this.page.fill('#username', username);
      await this.page.fill('#password', password);
      await this.page.click('button[type="submit"], #kc-login');
      
      // Wait for redirect back to app
      await this.page.waitForURL('http://localhost:5000/**', { timeout: 15000 });
    } catch (error) {
      // If testuser doesn't exist, provide helpful error message
      throw new Error(`Login failed for user '${username}'. Please run setup-test-user.bat to create the test user, or use existing Keycloak credentials.`);
    }
  }

  async logout() {
    // Look for logout button/link - try different selectors
    const logoutSelectors = [
      '[data-testid="logout-button"]',
      'button:has-text("Logout")',
      'button:has-text("Sign Out")',
      'a:has-text("Logout")',
      'a:has-text("Sign Out")',
      '.logout-button',
      '#logout-button'
    ];

    let logoutElement = null;
    for (const selector of logoutSelectors) {
      try {
        logoutElement = await this.page.waitForSelector(selector, { timeout: 2000 });
        break;
      } catch {
        continue;
      }
    }

    if (!logoutElement) {
      throw new Error('Logout button not found. Available elements: ' + 
        await this.page.evaluate(() => 
          Array.from(document.querySelectorAll('button, a'))
            .map(el => el.textContent?.trim())
            .filter(text => text && text.length > 0)
            .join(', ')
        )
      );
    }

    await logoutElement.click();
  }

  async refreshPage() {
    await this.page.reload();
  }

  async checkApiEndpoint(endpoint: string) {
    const response = await this.page.request.get(endpoint);
    return response.status();
  }

  async getCurrentUrl() {
    return this.page.url();
  }

  async takeScreenshot(name: string) {
    await this.page.screenshot({ path: `test-results/${name}.png`, fullPage: true });
  }

  async getPageContent() {
    return await this.page.textContent('body');
  }

  async waitForElement(selector: string, timeout = 5000) {
    await this.page.waitForSelector(selector, { timeout });
  }

  async isAuthenticated() {
    try {
      const response = await this.page.request.get('/auth/user');
      return response.status() === 200;
    } catch {
      return false;
    }
  }
}

test.describe('SystemInstaller Logout Functionality', () => {
  let logoutPage: LogoutTestPage;

  test.beforeEach(async ({ page }) => {
    logoutPage = new LogoutTestPage(page);
  });

  test('CRITICAL: Complete logout flow should prevent automatic re-authentication', async ({ page, context }) => {
    test.info().annotations.push({
      type: 'issue',
      description: 'User reports that after logout, F5 refresh automatically logs them back in'
    });

    console.log('🔄 Step 1: Navigate to application');
    await logoutPage.navigateToApp();
    await logoutPage.takeScreenshot('01-initial-navigation');

    console.log('🔄 Step 2: Verify we see login page');
    const initialPageType = await logoutPage.waitForLoginPage();
    console.log(`Initial page type: ${initialPageType}`);
    await logoutPage.takeScreenshot('02-login-page');

    if (initialPageType === 'keycloak-login') {
      console.log('🔄 Step 3: Login with admin credentials');
      await logoutPage.login('admin', 'admin');
      await logoutPage.takeScreenshot('03-after-login');

      console.log('🔄 Step 4: Wait for dashboard');
      await logoutPage.waitForDashboard();
      await logoutPage.takeScreenshot('04-dashboard-loaded');
    } else {
      console.log('⚠️  Already on welcome page - checking if authenticated');
      const isAuth = await logoutPage.isAuthenticated();
      console.log(`Is authenticated: ${isAuth}`);
    }

    console.log('🔄 Step 5: Verify user is authenticated');
    const authStatus = await logoutPage.checkApiEndpoint('/auth/user');
    expect(authStatus).toBe(200);
    console.log(`Authentication status: ${authStatus}`);

    console.log('🔄 Step 6: Perform logout');
    try {
      await logoutPage.logout();
      await logoutPage.takeScreenshot('05-logout-clicked');
      
      // Wait for logout to complete (may redirect to Keycloak logout)
      await page.waitForTimeout(2000);
      await logoutPage.takeScreenshot('06-after-logout-redirect');
      
    } catch (error) {
      console.log('❌ Logout failed:', error);
      await logoutPage.takeScreenshot('05-logout-failed');
      throw error;
    }

    console.log('🔄 Step 7: Wait for logout to complete');
    // Wait a bit for logout to complete
    await page.waitForTimeout(3000);
    
    console.log('🔄 Step 8: Check current page after logout');
    const currentUrl = await logoutPage.getCurrentUrl();
    console.log(`Current URL after logout: ${currentUrl}`);
    const pageContent = await logoutPage.getPageContent();
    console.log(`Page content contains: ${pageContent?.substring(0, 200)}...`);
    await logoutPage.takeScreenshot('07-after-logout-wait');

    console.log('🔄 Step 9: CRITICAL TEST - Refresh the page (F5)');
    await logoutPage.refreshPage();
    await page.waitForTimeout(2000);
    await logoutPage.takeScreenshot('08-after-f5-refresh');

    console.log('🔄 Step 10: Verify we are NOT automatically logged back in');
    const finalPageType = await logoutPage.waitForLoginPage();
    console.log(`Final page type after F5: ${finalPageType}`);
    
    // The critical assertion - we should see login page, not dashboard
    const postRefreshAuth = await logoutPage.checkApiEndpoint('/auth/user');
    console.log(`Auth status after F5: ${postRefreshAuth}`);
    
    // This should FAIL with current implementation
    expect(postRefreshAuth).toBe(401);
    
    console.log('🔄 Step 11: Verify protected endpoints are inaccessible');
    const protectedStatus = await logoutPage.checkApiEndpoint('/api/protected');
    expect(protectedStatus).toBe(401);

    console.log('✅ Test completed - logout should be complete and persistent');
  });

  test('Multiple tab session termination test', async ({ browser }) => {
    const context = await browser.newContext();
    const page1 = await context.newPage();
    const page2 = await context.newPage();

    const logout1 = new LogoutTestPage(page1);
    const logout2 = new LogoutTestPage(page2);

    // Login in first tab
    await logout1.navigateToApp();
    const pageType = await logout1.waitForLoginPage();
    if (pageType === 'keycloak-login') {
      await logout1.login('admin', 'admin');
      await logout1.waitForDashboard();
    }

    // Verify authentication in first tab
    expect(await logout1.checkApiEndpoint('/auth/user')).toBe(200);

    // Open second tab and verify session sharing
    await logout2.navigateToApp();
    await page2.waitForTimeout(2000);
    const secondTabAuth = await logout2.checkApiEndpoint('/auth/user');
    console.log(`Second tab auth status: ${secondTabAuth}`);

    // Logout from first tab
    await logout1.logout();
    await page1.waitForTimeout(3000);

    // Verify both tabs are logged out
    expect(await logout1.checkApiEndpoint('/auth/user')).toBe(401);
    
    await logout2.refreshPage();
    await page2.waitForTimeout(2000);
    expect(await logout2.checkApiEndpoint('/auth/user')).toBe(401);

    await context.close();
  });

  test('Gateway logout endpoint debugging', async ({ page }) => {
    // This test will help us see what's happening in the logs
    await logoutPage.navigateToApp();
    
    const pageType = await logoutPage.waitForLoginPage();
    if (pageType === 'keycloak-login') {
      await logoutPage.login('admin', 'admin');
      await logoutPage.waitForDashboard();
    }

    // Direct call to logout endpoint
    console.log('🔄 Testing direct logout endpoint call');
    const logoutResponse = await page.request.get('/auth/logout');
    console.log(`Direct logout response status: ${logoutResponse.status()}`);
    console.log(`Direct logout response URL: ${logoutResponse.url()}`);

    // Check if we get redirected to Keycloak logout
    const finalUrl = logoutResponse.url();
    expect(finalUrl).toContain('localhost:8082');
    expect(finalUrl).toContain('/protocol/openid-connect/logout');
  });
});
