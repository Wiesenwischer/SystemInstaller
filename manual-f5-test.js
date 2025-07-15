const { chromium } = require('playwright');

async function testF5RefreshScenario() {
    console.log('🔄 Testing F5 refresh scenario manually...');
    
    const browser = await chromium.launch({ 
        headless: false,
        slowMo: 2000 // Slow down to see what's happening
    });
    
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Set up console logging
        page.on('console', msg => console.log('🖥️ Browser:', msg.text()));
        
        // Step 1: Navigate to login
        console.log('1️⃣ Navigating to login...');
        await page.goto('http://localhost:5000/auth/login');
        
        // Wait for redirect to Keycloak
        console.log('2️⃣ Waiting for Keycloak login page...');
        await page.waitForURL('**/protocol/openid-connect/auth**', { timeout: 10000 });
        
        const keycloakUrl = page.url();
        console.log('📍 Keycloak URL:', keycloakUrl);
        
        // Step 3: Simulate F5 refresh
        console.log('3️⃣ Pressing F5 to refresh...');
        await page.keyboard.press('F5');
        
        // Wait for the page to reload
        await page.waitForTimeout(3000);
        
        const afterF5Url = page.url();
        console.log('📍 URL after F5:', afterF5Url);
        
        // Check the page content
        const pageContent = await page.textContent('body');
        console.log('📄 Page content preview:', pageContent.substring(0, 200) + '...');
        
        // Check if we got an error
        if (pageContent.includes('invalid_request') || pageContent.includes('Invalid request')) {
            console.log('❌ Found invalid_request error!');
            
            // Now try to navigate back to our app to see if our handler kicks in
            console.log('4️⃣ Trying to navigate back to app...');
            await page.goto('http://localhost:5000/');
            
            // Wait a bit to see what happens
            await page.waitForTimeout(3000);
            
            const finalUrl = page.url();
            console.log('📍 Final URL:', finalUrl);
            
            const finalContent = await page.textContent('body');
            console.log('📄 Final page content:', finalContent.substring(0, 200) + '...');
            
        } else {
            console.log('✅ No invalid_request error detected');
        }
        
        // Let's also try entering credentials after F5 to see if it works
        console.log('5️⃣ Trying to login after F5...');
        if (page.url().includes('protocol/openid-connect/auth')) {
            await page.fill('input[name="username"]', 'admin');
            await page.fill('input[name="password"]', 'admin');
            await page.click('input[type="submit"]');
            
            // Wait for result
            await page.waitForTimeout(5000);
            
            const loginResultUrl = page.url();
            console.log('📍 Login result URL:', loginResultUrl);
            
            if (loginResultUrl.includes('localhost:5000')) {
                console.log('✅ Login successful after F5');
            } else {
                console.log('❌ Login failed after F5');
            }
        }
        
        // Keep browser open for manual inspection
        console.log('🔍 Keeping browser open for 30 seconds for manual inspection...');
        await page.waitForTimeout(30000);
        
        return true;
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
        return false;
    } finally {
        await browser.close();
    }
}

// Run the test
testF5RefreshScenario().catch(console.error);
