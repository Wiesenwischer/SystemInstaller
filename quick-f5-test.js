const { chromium } = require('playwright');

async function quickF5Test() {
    console.log('⚡ Quick F5 refresh test for login screen...');
    
    const browser = await chromium.launch({ 
        headless: true, // Run headless for speed
        timeout: 30000
    });
    
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Set up error detection
        let hasInvalidRequestError = false;
        page.on('response', response => {
            if (response.url().includes('invalid_request') || response.status() >= 400) {
                console.log('⚠️ Error response detected:', response.url(), response.status());
            }
        });
        
        // Navigate to login
        console.log('1️⃣ Navigating to login...');
        await page.goto('http://localhost:5000/auth/login');
        
        // Wait for Keycloak login page (can be on different hosts)
        await page.waitForURL('**/protocol/openid-connect/auth**', { timeout: 10000 });
        console.log('✅ Reached Keycloak login page');
        
        // Perform F5 refresh
        console.log('2️⃣ Performing F5 refresh...');
        await page.reload();
        
        // Wait and check result
        await page.waitForTimeout(3000);
        
        const currentUrl = page.url();
        const pageContent = await page.textContent('body');
        
        console.log('📍 URL after F5:', currentUrl);
        
        // Check for invalid_request error
        if (pageContent.includes('invalid_request') || pageContent.includes('Invalid request')) {
            console.log('❌ FAIL: Found invalid_request error after F5 refresh');
            console.log('Error content:', pageContent.substring(0, 200) + '...');
            return false;
        }
        
        // Check if we're in a good state
        if (currentUrl.includes('protocol/openid-connect/auth') ||
            currentUrl.includes('localhost:5000/auth/login') ||
            currentUrl.includes('localhost:5000/')) {
            console.log('✅ PASS: F5 refresh handled correctly');
            
            // Try to continue with login to verify flow still works
            if (currentUrl.includes('protocol/openid-connect/auth')) {
                console.log('3️⃣ Testing login flow after F5 refresh...');
                await page.fill('input[name="username"]', 'admin');
                await page.fill('input[name="password"]', 'admin');
                await page.click('input[type="submit"]');
                
                // Wait for successful login
                await page.waitForURL('http://localhost:5000/', { timeout: 15000 });
                console.log('✅ Login flow works after F5 refresh');
            }
            
            return true;
        } else {
            console.log('❌ FAIL: Unexpected URL after F5 refresh:', currentUrl);
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error.message);
        return false;
    } finally {
        await browser.close();
    }
}

// Run the quick test
quickF5Test().then(result => {
    if (result) {
        console.log('\n🎉 F5 refresh fix is working correctly!');
        process.exit(0);
    } else {
        console.log('\n❌ F5 refresh fix needs attention!');
        process.exit(1);
    }
}).catch(error => {
    console.error('Test runner error:', error);
    process.exit(1);
});
