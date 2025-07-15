#!/usr/bin/env node

/**
 * SystemInstaller Logout E2E Test
 * Complete behavior-driven test that demonstrates the logout bug
 * Just run: node complete-logout-test.js
 */

// Try to load playwright, install if missing
let playwright;
try {
  playwright = require('playwright');
} catch (error) {
  console.log('❌ Playwright not found. Installing...');
  console.log('Run: npm install playwright');
  console.log('Then: npx playwright install');
  process.exit(1);
}

const { chromium } = playwright;

class LogoutBehaviorTest {
  constructor() {
    this.browser = null;
    this.page = null;
    this.testResults = {
      steps: [],
      screenshots: [],
      finalResult: null,
      bugConfirmed: false,
      logoutFailed: false
    };
  }

  log(message) {
    console.log(message);
    this.testResults.steps.push(`${new Date().toISOString()}: ${message}`);
  }

  async screenshot(name, description) {
    try {
      const filename = `test-${Date.now()}-${name}.png`;
      await this.page.screenshot({ path: filename, fullPage: true });
      this.log(`📸 Screenshot: ${filename} - ${description}`);
      this.testResults.screenshots.push({ name, filename, description });
    } catch (error) {
      this.log(`❌ Screenshot failed: ${error.message}`);
    }
  }

  async setup() {
    this.log('🚀 Starting SystemInstaller Logout Behavior Test');
    this.log('==================================================');
    
    this.browser = await chromium.launch({ 
      headless: false,
      slowMo: 500 // Slow down for visibility
    });
    this.page = await this.browser.newPage();
    
    // Set longer timeout
    this.page.setDefaultTimeout(30000);
  }

  async teardown() {
    if (this.browser) {
      await this.browser.close();
    }
    
    this.log('');
    this.log('📊 TEST SUMMARY');
    this.log('===============');
    this.log(`Total steps executed: ${this.testResults.steps.length}`);
    this.log(`Screenshots taken: ${this.testResults.screenshots.length}`);
    
    if (this.testResults.logoutFailed) {
      this.log('🔴 CRITICAL ERROR: 404 error page displayed during logout process');
      this.log('   This indicates a broken logout endpoint or routing issue');
    } else if (this.testResults.bugConfirmed) {
      this.log('❌ BUG CONFIRMED: Logout does not properly terminate Keycloak session');
      this.log('   After logout + F5 refresh, user is automatically logged back in');
    } else {
      this.log('✅ LOGOUT WORKING: User properly logged out and stays logged out');
    }
  }

  async scenario_NavigateToApplication() {
    this.log('');
    this.log('🔄 SCENARIO: Navigate to SystemInstaller Application');
    this.log('Given I am a user of SystemInstaller');
    this.log('When I navigate to the application URL');
    
    await this.page.goto('http://localhost:5000');
    await this.page.waitForTimeout(3000);
    await this.screenshot('01-navigation', 'Initial navigation to app');
    
    const currentUrl = this.page.url();
    this.log(`Then I should see the application at: ${currentUrl}`);
    
    return currentUrl;
  }

  async scenario_LoginProcess() {
    this.log('');
    this.log('🔄 SCENARIO: User Authentication');
    this.log('Given I need to authenticate');
    this.log('When I am redirected to Keycloak login');
    
    // Check if we're on Keycloak login page
    const isKeycloakLogin = this.page.url().includes('localhost:8082') || 
                           await this.page.locator('h1:has-text("Sign In")').isVisible();
    
    if (!isKeycloakLogin) {
      this.log('⚠️  Already authenticated or on welcome page');
      return 'already-authenticated';
    }
    
    await this.screenshot('02-login-page', 'Keycloak login page displayed');
    
    this.log('And I enter valid credentials (admin/admin123)');
    await this.page.fill('#username', 'admin');
    await this.page.fill('#password', 'admin123');
    
    this.log('And I submit the login form');
    await this.page.click('button[type="submit"], #kc-login');
    
    this.log('Then I should be redirected back to SystemInstaller');
    await this.page.waitForURL('http://localhost:5000/**', { timeout: 15000 });
    await this.screenshot('03-after-login', 'Successfully authenticated and redirected');
    
    return 'login-successful';
  }

  async scenario_VerifyAuthentication() {
    this.log('');
    this.log('🔄 SCENARIO: Verify User is Authenticated');
    this.log('Given I have completed the login process');
    this.log('When I check my authentication status');
    
    const authResponse = await this.page.request.get('http://localhost:5000/auth/user');
    const authStatus = authResponse.status();
    
    this.log(`Then my authentication status should be: ${authStatus}`);
    
    if (authStatus === 200) {
      const userInfo = await authResponse.json();
      this.log(`And I should be logged in as: ${userInfo.username || userInfo.sub || 'Unknown'}`);
      await this.screenshot('04-authenticated', 'User successfully authenticated');
      return userInfo;
    } else {
      this.log('❌ Authentication verification failed');
      await this.screenshot('04-auth-failed', 'Authentication verification failed');
      throw new Error('User is not properly authenticated');
    }
  }

  async scenario_PerformLogout() {
    this.log('');
    this.log('🔄 SCENARIO: User Logout Process');
    this.log('Given I am authenticated and on the dashboard');
    this.log('When I initiate the logout process');
    
    // Try to find logout button
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
        if (await this.page.locator(selector).isVisible()) {
          this.log(`And I click the logout button: ${selector}`);
          await this.page.locator(selector).click();
          logoutFound = true;
          break;
        }
      } catch {
        continue;
      }
    }
    
    if (!logoutFound) {
      this.log('And I navigate directly to logout URL (no button found)');
      await this.page.goto('http://localhost:5000/auth/logout');
    }
    
    await this.screenshot('05-logout-initiated', 'Logout process initiated');
    
    this.log('Then the logout process should begin');
    await this.page.waitForTimeout(3000);
    
    // Check for 404 error page content
    const pageContent = await this.page.content();
    const pageTitle = await this.page.title();
    const statusText = await this.page.locator('body').textContent();
    
    if (pageContent.includes('404') || pageTitle.includes('404') || statusText.includes('404') || 
        pageContent.includes('Not Found') || pageTitle.includes('Not Found') || statusText.includes('Not Found')) {
      this.log('💥 ERROR: 404 PAGE DETECTED!');
      this.log(`Page title: ${pageTitle}`);
      this.log(`Page contains 404 error content`);
      throw new Error('404 error page displayed during logout process');
    }
    
    const afterLogoutUrl = this.page.url();
    this.log(`And I should be at: ${afterLogoutUrl}`);
    await this.screenshot('06-after-logout', 'After logout redirect completed');
    
    return afterLogoutUrl;
  }

  async scenario_CriticalRefreshTest() {
    this.log('');
    this.log('🔄 SCENARIO: Critical F5 Refresh Test');
    this.log('Given I have just completed the logout process');
    this.log('When I refresh the page (F5) to simulate normal user behavior');
    
    await this.page.reload();
    await this.page.waitForTimeout(3000);
    await this.screenshot('07-after-f5-refresh', 'After F5 refresh - CRITICAL TEST');
    
    this.log('Then I should verify my authentication status');
    const authAfterF5 = await this.page.request.get('http://localhost:5000/auth/user');
    const authStatus = authAfterF5.status();
    
    this.log(`And my authentication status should be: ${authStatus}`);
    
    const currentUrl = this.page.url();
    this.log(`And the current URL should be: ${currentUrl}`);
    
    // This is the critical test
    if (authStatus === 200) {
      this.log('❌ CRITICAL BUG DETECTED!');
      this.log('   User is automatically re-authenticated after logout + F5');
      this.log('   This indicates Keycloak session was not properly terminated');
      this.testResults.bugConfirmed = true;
      this.testResults.finalResult = 'BUG_CONFIRMED';
      
      try {
        const userInfo = await authAfterF5.json();
        this.log(`   Still authenticated as: ${userInfo.username || userInfo.sub}`);
      } catch (e) {
        this.log('   Authentication detected but user info unavailable');
      }
    } else {
      this.log('✅ LOGOUT WORKING CORRECTLY!');
      this.log('   User remains logged out after F5 refresh');
      this.log('   Keycloak session was properly terminated');
      this.testResults.finalResult = 'LOGOUT_WORKING';
    }
    
    return { authStatus, currentUrl };
  }

  async scenario_VerifyProtectedEndpoints() {
    this.log('');
    this.log('🔄 SCENARIO: Verify Protected Endpoints are Inaccessible');
    this.log('Given I should be logged out');
    this.log('When I try to access protected endpoints');
    
    const protectedEndpoints = ['http://localhost:5000/auth/user', 'http://localhost:5000/api/protected'];
    
    for (const endpoint of protectedEndpoints) {
      const response = await this.page.request.get(endpoint);
      const status = response.status();
      this.log(`Then ${endpoint} should return: ${status} (expected: 401)`);
      
      if (status !== 401) {
        this.log(`❌ Protected endpoint ${endpoint} is still accessible!`);
        this.testResults.bugConfirmed = true;
      }
    }
  }

  async scenario_AttemptReaccess() {
    this.log('');
    this.log('🔄 SCENARIO: Attempt to Re-access Application');
    this.log('Given I should be completely logged out');
    this.log('When I navigate to the application again');
    
    await this.page.goto('http://localhost:5000');
    await this.page.waitForTimeout(3000);
    await this.screenshot('08-reaccess-attempt', 'Attempting to re-access application');
    
    const finalUrl = this.page.url();
    this.log(`Then I should be at: ${finalUrl}`);
    
    if (finalUrl.includes('localhost:8082')) {
      this.log('✅ Correctly redirected to Keycloak for re-authentication');
      this.log('And I should be required to enter credentials again');
    } else {
      this.log('❌ Not redirected to login - may still be authenticated');
      this.testResults.bugConfirmed = true;
    }
    
    return finalUrl;
  }

  async runCompleteTest() {
    try {
      await this.setup();
      
      // Execute all scenarios in sequence
      await this.scenario_NavigateToApplication();
      const loginResult = await this.scenario_LoginProcess();
      
      if (loginResult !== 'already-authenticated') {
        await this.scenario_VerifyAuthentication();
      } else {
        // Check if already authenticated
        const authCheck = await this.page.request.get('http://localhost:5000/auth/user');
        if (authCheck.status() !== 200) {
          // Not authenticated, need to login
          await this.scenario_LoginProcess();
          await this.scenario_VerifyAuthentication();
        }
      }
      
      try {
        await this.scenario_PerformLogout();
      } catch (error) {
        if (error.message.includes('404 error page displayed during logout process')) {
          this.log('💥 CRITICAL ERROR: 404 detected during logout - this is a bug!');
          this.testResults.logoutFailed = true;
          this.testResults.bugConfirmed = true;
          await this.screenshot('99-logout-404-error', '404 error during logout');
          // Don't continue with other tests if logout failed with 404
          await this.teardown();
          return;
        } else {
          throw error; // Re-throw other errors
        }
      }
      
      const criticalResult = await this.scenario_CriticalRefreshTest();
      await this.scenario_VerifyProtectedEndpoints();
      await this.scenario_AttemptReaccess();
      
    } catch (error) {
      this.log(`❌ Test execution failed: ${error.message}`);
      await this.screenshot('99-error', 'Test execution error');
    } finally {
      await this.teardown();
    }
  }
}

// Run the test if this script is executed directly
if (require.main === module) {
  const test = new LogoutBehaviorTest();
  test.runCompleteTest().catch(console.error);
}

module.exports = LogoutBehaviorTest;
