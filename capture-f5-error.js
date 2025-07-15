const { chromium } = require('playwright');

async function captureF5Error() {
    console.log('🔍 Capturing F5 error details...');
    
    const browser = await chromium.launch({ 
        headless: false,
        slowMo: 1000
    });
    
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Capture network requests
        page.on('response', response => {
            if (response.status() >= 400) {
                console.log(`❌ HTTP ${response.status()}: ${response.url()}`);
            }
        });
        
        // Capture console errors
        page.on('console', msg => {
            if (msg.type() === 'error') {
                console.log('🔴 Browser error:', msg.text());
            }
        });
        
        // Navigate to login
        await page.goto('http://localhost:5000/auth/login');
        await page.waitForURL('**/protocol/openid-connect/auth**', { timeout: 10000 });
        
        console.log('📍 Current URL:', page.url());
        
        // Press F5
        console.log('🔄 Pressing F5...');
        await page.keyboard.press('F5');
        
        // Wait for page to reload
        await page.waitForTimeout(3000);
        
        // Check the page content for errors
        const pageContent = await page.textContent('body');
        console.log('📄 Page content after F5:');
        console.log(pageContent);
        
        // Look for specific error patterns
        if (pageContent.includes('invalid_request')) {
            console.log('❌ Found invalid_request in page content');
        }
        
        if (pageContent.includes('Invalid request')) {
            console.log('❌ Found "Invalid request" in page content');
        }
        
        if (pageContent.includes('error')) {
            console.log('⚠️ Found "error" in page content');
        }
        
        console.log('📍 URL after F5:', page.url());
        
        // Try to navigate back to our app to trigger the error handler
        console.log('🔄 Navigating back to app to trigger error handler...');
        await page.goto('http://localhost:5000/');
        
        await page.waitForTimeout(5000);
        
        const finalUrl = page.url();
        const finalContent = await page.textContent('body');
        
        console.log('📍 Final URL:', finalUrl);
        console.log('📄 Final content preview:', finalContent.substring(0, 300));
        
        // Keep browser open for inspection
        await page.waitForTimeout(10000);
        
    } catch (error) {
        console.error('❌ Error:', error.message);
    } finally {
        await browser.close();
    }
}

captureF5Error().catch(console.error);
