const { chromium } = require('playwright');

(async () => {
    console.log('🔐 Testing logout flow specifically...');
    
    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext({
        ignoreHTTPSErrors: true
    });
    
    const page = await context.newPage();
    
    try {
        // Navigate to the application
        console.log('📍 Step 1: Navigate to gateway');
        await page.goto('http://localhost:5000');
        await page.waitForTimeout(2000);
        
        // Login
        console.log('📍 Step 2: Login');
        await page.waitForSelector('#username', { timeout: 5000 });
        await page.fill('#username', 'admin');
        await page.fill('#password', 'admin123');
        await page.click('button[type="submit"]');
        await page.waitForTimeout(3000);
        
        // Test logout
        console.log('📍 Step 3: Testing logout');
        await page.click('button:has-text("admin")');
        await page.waitForTimeout(1000);
        await page.click('button:has-text("Sign out")');
        
        // Wait for logout to complete and URL to change
        console.log('📍 Step 4: Waiting for logout redirect...');
        
        // Wait for the URL to change to indicate logout is processing
        await page.waitForFunction(() => {
            return window.location.href.includes('keycloak') || window.location.href.includes('8082');
        }, { timeout: 10000 });
        
        console.log('URL changed to:', page.url());
        
        // Wait a bit more for any final redirects
        await page.waitForTimeout(3000);
        
        const finalUrl = page.url();
        console.log('Final URL:', finalUrl);
        
        // Check if we're on the login page or if there's an error
        const pageContent = await page.content();
        
        // Log relevant parts of the page content for debugging
        if (pageContent.includes('invalid redirect uri')) {
            console.log('❌ Found "invalid redirect uri" error on page');
            console.log('Page title:', await page.title());
        } else if (pageContent.includes('You are logged out')) {
            console.log('✅ Successfully logged out - on Keycloak logout success page');
        } else if (finalUrl.includes('keycloak') && finalUrl.includes('auth')) {
            console.log('✅ Redirected to Keycloak login page');
        } else if (finalUrl.includes('keycloak')) {
            console.log('✅ On Keycloak page - logout completed');
            console.log('Page title:', await page.title());
        } else {
            console.log('⚠️ Unexpected state after logout');
            console.log('Page title:', await page.title());
        }
        
        // Take a screenshot for debugging
        await page.screenshot({ path: 'logout-result.png' });
        console.log('📸 Screenshot saved as logout-result.png');
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
        await page.screenshot({ path: 'error-screenshot.png' });
    } finally {
        await browser.close();
        console.log('🏁 Test completed');
    }
})();
