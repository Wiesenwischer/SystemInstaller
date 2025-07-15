const { chromium } = require('playwright');

async function testLogoutFlow() {
    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext();
    const page = await context.newPage();
    
    console.log('🧪 Testing logout flow...');
    
    try {
        // Step 1: Go to app and login
        console.log('1. Navigating to app...');
        await page.goto('http://localhost:5000');
        
        // Should be redirected to login
        await page.waitForURL(/host\.docker\.internal:8082.*auth/, { timeout: 10000 });
        console.log('✅ Redirected to Keycloak login');
        
        // Wait for the login form to be visible
        await page.waitForSelector('input[name="username"]', { timeout: 5000 });
        await page.waitForSelector('input[name="password"]', { timeout: 5000 });
        
        // Login
        await page.fill('input[name="username"]', 'admin');
        await page.fill('input[name="password"]', 'admin123');
        
        // Wait for submit button and click it - try different selectors
        let submitButton = null;
        try {
            await page.waitForSelector('input[type="submit"]', { timeout: 2000 });
            submitButton = 'input[type="submit"]';
        } catch {
            try {
                await page.waitForSelector('button[type="submit"]', { timeout: 2000 });
                submitButton = 'button[type="submit"]';
            } catch {
                try {
                    await page.waitForSelector('#kc-login', { timeout: 2000 });
                    submitButton = '#kc-login';
                } catch {
                    console.log('Could not find submit button, trying form submission');
                    await page.keyboard.press('Enter');
                }
            }
        }
        
        if (submitButton) {
            await page.click(submitButton);
        }
        
        // Should be redirected back to app
        await page.waitForURL('http://localhost:5000/', { timeout: 10000 });
        console.log('✅ Successfully logged in and redirected to app');
        
        // Step 2: Test logout
        console.log('2. Testing logout...');
        
        // Navigate to logout endpoint
        await page.goto('http://localhost:5000/auth/logout');
        
        // Wait for the logout redirect to complete - should end up back at the app or login page
        try {
            // Wait for either app root or login page
            await page.waitForURL(url => 
                url === 'http://localhost:5000/' || 
                url.includes('host.docker.internal:8082') && url.includes('auth') && !url.includes('logout')
            , { timeout: 10000 });
        } catch (error) {
            console.log('Waiting for redirect after logout...');
            await page.waitForTimeout(5000);
        }
        
        // Check final URL
        const finalUrl = page.url();
        console.log(`Final URL after logout: ${finalUrl}`);
        
        // Check if we're on login page or Keycloak logout page
        if (finalUrl.includes('host.docker.internal:8082') && finalUrl.includes('logout')) {
            console.log('❌ FAILED: Still on Keycloak logout page');
            return false;
        } else if (finalUrl.includes('host.docker.internal:8082') && finalUrl.includes('auth')) {
            console.log('✅ SUCCESS: Redirected to login page');
            return true;
        } else if (finalUrl === 'http://localhost:5000/') {
            // Check if we get redirected to login from root
            await page.waitForTimeout(2000);
            const afterRootUrl = page.url();
            console.log(`URL after accessing root: ${afterRootUrl}`);
            
            if (afterRootUrl.includes('host.docker.internal:8082') && afterRootUrl.includes('auth')) {
                console.log('✅ SUCCESS: Redirected to app root, then to login');
                return true;
            } else {
                console.log('❌ FAILED: At app root but not redirected to login');
                return false;
            }
        } else {
            console.log(`❌ FAILED: Unexpected URL: ${finalUrl}`);
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error.message);
        return false;
    } finally {
        await browser.close();
    }
}

// Run the test
testLogoutFlow().then(success => {
    if (success) {
        console.log('🎉 LOGOUT FLOW TEST PASSED');
        process.exit(0);
    } else {
        console.log('💀 LOGOUT FLOW TEST FAILED');
        process.exit(1);
    }
});
