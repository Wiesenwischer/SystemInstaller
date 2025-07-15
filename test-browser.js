const { chromium } = require('playwright');

async function testLogout() {
  let browser;
  let page;
  
  try {
    console.log('🚀 Testing login then logout to see if 404 error appears...');
    
    browser = await chromium.launch({ 
      headless: false,
      slowMo: 2000,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    console.log('✅ Browser launched successfully!');
    
    page = await browser.newPage();
    console.log('✅ New page created!');
    
    // Enhanced request/response logging
    page.on('request', request => {
      console.log(`📤 REQUEST: ${request.method()} ${request.url()}`);
    });
    
    page.on('response', response => {
      console.log(`📥 RESPONSE: ${response.status()} ${response.url()}`);
      if (response.status() >= 400) {
        console.log(`❌ ERROR RESPONSE: ${response.status()} - ${response.statusText()}`);
      }
    });
    
    page.on('pageerror', error => {
      console.log(`💥 PAGE ERROR: ${error.message}`);
    });
    
    console.log('🔐 First accessing gateway...');
    await page.goto('http://localhost:5000');
    
    console.log('⏳ Waiting to see if we get redirected to Keycloak...');
    try {
      // Wait for redirect to Keycloak and login - now using host.docker.internal
      await page.waitForURL('**/realms/systeminstaller/**', { timeout: 10000 });
      console.log('📍 Redirected to Keycloak login');
      
      // Take a screenshot to see the login page
      await page.screenshot({ path: 'keycloak-login.png' });
      console.log('📸 Screenshot saved as keycloak-login.png');
      
    } catch (error) {
      console.log('⚠️  No redirect to Keycloak - checking current page...');
      const currentUrl = page.url();
      console.log(`📍 Current URL: ${currentUrl}`);
      
      if (currentUrl === 'http://localhost:5000/') {
        console.log('✅ Already on gateway homepage - authentication might not be required');
        console.log('🔐 Let\'s try to access a protected endpoint to trigger authentication...');
        await page.goto('http://localhost:5000/api/protected');
        await page.waitForURL('**/realms/systeminstaller/**', { timeout: 10000 });
        console.log('📍 Now redirected to Keycloak login');
      } else {
        throw new Error(`Unexpected page: ${currentUrl}`);
      }
    }
    
    // Fill login form
    console.log('📝 Filling username...');
    await page.fill('#username', 'admin');
    console.log('📝 Filling password...');
    await page.fill('#password', 'admin123');
    
    // More robust button clicking with multiple selectors
    console.log('🔍 Looking for login button...');
    
    // First, let's inspect the entire page to understand the form structure
    const pageContent = await page.content();
    console.log('📄 Page contains form:', pageContent.includes('<form'));
    console.log('📄 Page contains submit:', pageContent.includes('submit'));
    console.log('📄 Page contains login:', pageContent.includes('login'));
    
    // Get all form elements for debugging
    const forms = await page.locator('form').all();
    console.log(`📋 Found ${forms.length} forms on the page`);
    
    for (let i = 0; i < forms.length; i++) {
      const action = await forms[i].getAttribute('action');
      const method = await forms[i].getAttribute('method');
      console.log(`Form ${i}: action="${action}", method="${method}"`);
    }
    
    // Get all input elements
    const allInputs = await page.locator('input').all();
    console.log(`📝 Found ${allInputs.length} input elements`);
    
    for (let i = 0; i < allInputs.length; i++) {
      const type = await allInputs[i].getAttribute('type');
      const name = await allInputs[i].getAttribute('name');
      const id = await allInputs[i].getAttribute('id');
      const value = await allInputs[i].getAttribute('value');
      console.log(`Input ${i}: type="${type}", name="${name}", id="${id}", value="${value}"`);
    }
    
    // Try different possible selectors for the login button
    const buttonSelectors = [
      'input[type="submit"]',
      'button[type="submit"]', 
      '#kc-login',
      'input[name="login"]',
      'button[name="login"]',
      '.btn-primary',
      '[data-testid="sign-in"]',
      'input[value*="Sign"]',
      'input[value*="Login"]',
      'button:has-text("Sign")',
      'button:has-text("Login")'
    ];
    
    let buttonFound = false;
    for (const selector of buttonSelectors) {
      try {
        const button = await page.locator(selector).first();
        const isVisible = await button.isVisible();
        const count = await page.locator(selector).count();
        console.log(`🔍 Selector "${selector}": count=${count}, visible=${isVisible}`);
        
        if (isVisible) {
          console.log(`✅ Found login button with selector: ${selector}`);
          await button.click();
          buttonFound = true;
          break;
        }
      } catch (e) {
        console.log(`❌ Button not found with selector: ${selector} - ${e.message}`);
      }
    }
    
    if (!buttonFound) {
      console.log('🔍 No standard button found, trying to find any clickable element...');
      const allButtons = await page.locator('button, input[type="submit"], input[type="button"]').all();
      console.log(`Found ${allButtons.length} potential buttons`);
      
      for (let i = 0; i < allButtons.length; i++) {
        const text = await allButtons[i].textContent();
        const value = await allButtons[i].getAttribute('value');
        const type = await allButtons[i].getAttribute('type');
        const name = await allButtons[i].getAttribute('name');
        console.log(`Button ${i}: text="${text}", value="${value}", type="${type}", name="${name}"`);
        
        if (text?.toLowerCase().includes('sign') || text?.toLowerCase().includes('login') || 
            value?.toLowerCase().includes('sign') || value?.toLowerCase().includes('login') ||
            type === 'submit') {
          console.log(`✅ Clicking button ${i} based on text/value/type`);
          await allButtons[i].click();
          buttonFound = true;
          break;
        }
      }
    }
    
    if (!buttonFound) {
      console.log('🔍 Still no button found, trying to submit form directly...');
      try {
        await page.keyboard.press('Enter');
        console.log('✅ Tried submitting form with Enter key');
        buttonFound = true;
      } catch (e) {
        console.log('❌ Could not submit with Enter key');
      }
    }
    
    if (!buttonFound) {
      console.log('❌ Dumping page HTML for debugging...');
      const html = await page.content();
      console.log('📄 First 1000 chars of page:', html.substring(0, 1000));
      throw new Error('Could not find login button');
    }
    
    console.log('⏳ Waiting for successful login...');
    
    // Wait a bit to see what happens after clicking
    await page.waitForTimeout(3000);
    
    // Take a screenshot after login attempt
    await page.screenshot({ path: 'after-login-attempt.png' });
    console.log('📸 Screenshot after login attempt saved as after-login-attempt.png');
    
    // Check current URL after login attempt
    const currentUrlAfterLogin = page.url();
    console.log(`📍 URL after login attempt: ${currentUrlAfterLogin}`);
    
    // Check for any error pages or authentication failures
    const pageTitle = await page.title();
    const pageText = await page.locator('body').textContent();
    
    console.log(`📝 Page title: ${pageTitle}`);
    
    if (pageText.includes('AuthenticationFailureException') || 
        pageText.includes('Correlation failed') ||
        pageText.includes('unhandled exception')) {
      console.log('❌ AUTHENTICATION ERROR DETECTED!');
      console.log('📄 Error details:');
      
      // Extract error details
      const lines = pageText.split('\n');
      for (const line of lines) {
        if (line.includes('AuthenticationFailureException') || 
            line.includes('Correlation') ||
            line.includes('Exception') ||
            line.includes('Error')) {
          console.log(`   ${line.trim()}`);
        }
      }
      
      // Take screenshot of error
      await page.screenshot({ path: 'authentication-error.png' });
      console.log('📸 Authentication error screenshot saved');
      
      throw new Error('Authentication failed with correlation error');
    }
    
    if (currentUrlAfterLogin.includes('keycloak')) {
      console.log('⚠️  Still on Keycloak page, login might have failed');
      
      // Check for error messages
      const errorElements = await page.locator('.alert-error, .error, #input-error').all();
      for (let i = 0; i < errorElements.length; i++) {
        const errorText = await errorElements[i].textContent();
        console.log(`❌ Error message ${i}: ${errorText}`);
      }
      
      // Let's try a different approach - maybe the credentials are wrong
      console.log('🔄 Trying alternative credentials or checking form validation...');
      
      // Check if username/password fields still exist and have values
      const usernameValue = await page.locator('#username').inputValue();
      const passwordValue = await page.locator('#password').inputValue();
      console.log(`📝 Username field value: "${usernameValue}"`);
      console.log(`📝 Password field value: "${passwordValue}"`);
      
      throw new Error('Login failed - still on Keycloak page');
    }
    
    try {
      await page.waitForURL('http://localhost:5000/', { timeout: 10000 });
      console.log('✅ Successfully logged in and redirected to gateway homepage!');
    } catch (e) {
      console.log(`⚠️  Not redirected to gateway homepage. Current URL: ${page.url()}`);
      
      // If we're still on the protected endpoint, that's wrong - we should be on the homepage
      if (page.url().includes('/api/protected')) {
        console.log('❌ ERROR: After login, user should be redirected to homepage, not API endpoint!');
        // Let's manually navigate to the homepage to continue the test
        await page.goto('http://localhost:5000/');
        await page.waitForTimeout(2000);
        console.log('🔄 Manually navigated to homepage to continue test');
      }
    }
    
    // Verify we're on the dashboard/homepage with user dropdown
    console.log('🏠 Verifying we are on the dashboard homepage...');
    const isOnHomepage = page.url() === 'http://localhost:5000/' || page.url() === 'http://localhost:5000';
    console.log(`📍 On homepage: ${isOnHomepage}, URL: ${page.url()}`);
    
    // Look for user dropdown or sign out button
    console.log('� Looking for user dropdown or sign out menu...');
    
    // Take screenshot of the homepage
    await page.screenshot({ path: 'homepage-after-login.png' });
    console.log('📸 Homepage screenshot saved');
    
    // Try to find and click the user dropdown
    const userDropdownSelectors = [
      '[data-testid="user-dropdown"]',
      '.user-dropdown',
      '.dropdown-toggle',
      'button:has-text("admin")',
      'button:has-text("user")',
      '[aria-label="User menu"]',
      '.navbar .dropdown',
      '.user-menu'
    ];
    
    let dropdownFound = false;
    for (const selector of userDropdownSelectors) {
      try {
        const dropdown = await page.locator(selector).first();
        if (await dropdown.isVisible()) {
          console.log(`✅ Found user dropdown with selector: ${selector}`);
          await dropdown.click();
          await page.waitForTimeout(1000); // Wait for dropdown to open
          dropdownFound = true;
          break;
        }
      } catch (e) {
        console.log(`❌ User dropdown not found with selector: ${selector}`);
      }
    }
    
    if (!dropdownFound) {
      console.log('⚠️  Could not find user dropdown, looking for direct sign out button...');
    }
    
    // Now look for sign out menu item
    const signOutSelectors = [
      'a:has-text("Sign Out")',
      'button:has-text("Sign Out")',
      'a:has-text("Logout")', 
      'button:has-text("Logout")',
      '[data-testid="sign-out"]',
      '.sign-out',
      '.logout'
    ];
    
    let signOutFound = false;
    for (const selector of signOutSelectors) {
      try {
        const signOutButton = await page.locator(selector).first();
        if (await signOutButton.isVisible()) {
          console.log(`✅ Found sign out button with selector: ${selector}`);
          await signOutButton.click();
          signOutFound = true;
          break;
        }
      } catch (e) {
        console.log(`❌ Sign out button not found with selector: ${selector}`);
      }
    }
    
    if (!signOutFound) {
      console.log('⚠️  Could not find sign out menu item, falling back to direct logout URL...');
      await page.goto('http://localhost:5000/auth/logout');
    } else {
      console.log('✅ Clicked sign out menu item');
    }
    
    console.log('🚪 Testing logout process...');
    
    // Wait a moment to see what happens
    await page.waitForTimeout(3000);
    
    const currentUrl = page.url();
    const title = await page.title();
    const bodyText = await page.locator('body').textContent();
    
    console.log('\n📊 LOGOUT RESULTS:');
    console.log('==================');
    console.log(`📍 Current URL: ${currentUrl}`);
    console.log(`📝 Page title: ${title}`);
    console.log(`📄 Body content: ${bodyText.substring(0, 300)}`);
    
    if (bodyText.includes('404') || title.includes('404')) {
      console.log('❌ 404 ERROR DETECTED! This is the problem.');
      console.log('❌ The logout endpoint is returning a 404 error.');
      return false;
    } else if (currentUrl.includes('keycloak') || 
               currentUrl.includes('host.docker.internal:8082') ||
               currentUrl.includes('realms/systeminstaller')) {
      console.log('✅ Good! Logout redirected to Keycloak.');
      return true;
    } else {
      console.log('⚠️  Unexpected logout behavior.');
      return false;
    }
    
  } catch (error) {
    console.error('❌ Test failed:', error.message);
    return false;
  } finally {
    if (page) {
      console.log('\n⏳ Keeping browser open for 5 seconds for inspection...');
      await page.waitForTimeout(5000);
    }
    if (browser) {
      await browser.close();
      console.log('✅ Test completed!');
    }
  }
}

// Run the test
testLogout().then(success => {
  if (success) {
    console.log('\n🎉 LOGOUT TEST PASSED! 🎉');
    process.exit(0);
  } else {
    console.log('\n💥 LOGOUT TEST FAILED! 💥');
    process.exit(1);
  }
});
