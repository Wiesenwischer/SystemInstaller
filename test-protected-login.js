const { chromium } = require('playwright');

async function testProtectedLogin() {
    console.log('🔐 Testing protected login page...');
    
    const browser = await chromium.launch({ 
        headless: false,
        slowMo: 1000 
    });
    
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Navigate to protected login page
        console.log('1️⃣ Navigating to protected login page...');
        await page.goto('http://localhost:5000/auth/login-with-protection');
        
        // Wait for page to load
        await page.waitForTimeout(2000);
        
        console.log('2️⃣ Testing F5 prevention...');
        // Try to press F5 - should be prevented
        await page.keyboard.press('F5');
        
        // Wait a moment to see if alert appears
        await page.waitForTimeout(1000);
        
        console.log('3️⃣ Clicking login button...');
        // Click the login button
        await page.click('.button');
        
        // Wait for redirect to Keycloak
        await page.waitForTimeout(3000);
        
        const currentUrl = page.url();
        console.log('📍 Current URL:', currentUrl);
        
        if (currentUrl.includes('protocol/openid-connect/auth')) {
            console.log('✅ Successfully redirected to Keycloak login');
            
            // Try F5 here to see if it still causes issues
            console.log('4️⃣ Testing F5 on Keycloak page...');
            await page.keyboard.press('F5');
            await page.waitForTimeout(2000);
            
            const pageContent = await page.textContent('body');
            if (pageContent.includes('invalid_request')) {
                console.log('❌ Still getting invalid_request error');
            } else {
                console.log('✅ No invalid_request error detected');
            }
        }
        
        // Keep browser open for inspection
        console.log('🔍 Keeping browser open for inspection...');
        await page.waitForTimeout(15000);
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
    } finally {
        await browser.close();
    }
}

testProtectedLogin().catch(console.error);
