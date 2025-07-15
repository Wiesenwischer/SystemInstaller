const { chromium } = require('playwright');

async function testF5RefreshOnLogin() {
    console.log('🔄 Testing F5 refresh on login screen...');
    
    const browser = await chromium.launch({ 
        headless: false,
        slowMo: 1000 // Slow down for better visibility
    });
    
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Enable console logging to see what's happening
        page.on('console', msg => {
            if (msg.type() === 'error') {
                console.log('❌ Browser console error:', msg.text());
            }
        });
        
        // Step 1: Navigate to the app (should redirect to login)
        console.log('1️⃣ Navigating to app root...');
        await page.goto('http://localhost:5000');
        
        // Wait for redirect to Keycloak login page
        console.log('2️⃣ Waiting for redirect to Keycloak login page...');
        await page.waitForURL('**/auth/realms/systeminstaller/protocol/openid-connect/auth**', { timeout: 10000 });
        console.log('✅ Successfully redirected to Keycloak login page');
        
        // Step 2: Now we're on the login page - simulate F5 refresh
        console.log('3️⃣ Simulating F5 refresh on login page...');
        await page.reload();
        
        // Step 3: Check what happens after F5 refresh
        console.log('4️⃣ Checking page state after F5 refresh...');
        
        // Wait a moment for any redirects or error handling
        await page.waitForTimeout(2000);
        
        const currentUrl = page.url();
        console.log('📍 Current URL after F5 refresh:', currentUrl);
        
        // Check if we're still on a login-related page or if we got an error
        if (currentUrl.includes('auth/realms/systeminstaller/protocol/openid-connect/auth')) {
            console.log('✅ Still on Keycloak login page - this is expected');
        } else if (currentUrl.includes('localhost:5000/auth/login')) {
            console.log('✅ Redirected back to our auth/login endpoint - this is the fix working!');
        } else if (currentUrl.includes('localhost:5000/auth/error')) {
            console.log('⚠️ Redirected to error page - let\'s check the error message');
            const errorText = await page.textContent('body');
            console.log('Error page content:', errorText);
        } else {
            console.log('❓ Unexpected URL after F5 refresh:', currentUrl);
        }
        
        // Step 4: Check for any error messages on the page
        const pageText = await page.textContent('body');
        if (pageText.includes('invalid_request') || pageText.includes('Invalid request')) {
            console.log('❌ Found "invalid_request" error - F5 refresh fix is not working properly');
            return false;
        } else if (pageText.includes('authentication_failed')) {
            console.log('⚠️ Found authentication_failed error - checking if this is handled gracefully');
            // This might be OK if it's handled gracefully
        }
        
        // Step 5: Try to navigate back to login after F5 refresh
        console.log('5️⃣ Testing navigation back to login after F5 refresh...');
        await page.goto('http://localhost:5000/auth/login');
        
        // Wait for redirect to Keycloak login page again
        await page.waitForURL('**/auth/realms/systeminstaller/protocol/openid-connect/auth**', { timeout: 10000 });
        console.log('✅ Successfully navigated back to login page after F5 refresh');
        
        // Step 6: Try to complete a normal login flow to ensure everything still works
        console.log('6️⃣ Testing normal login flow after F5 refresh...');
        
        // Fill in login credentials
        await page.fill('input[name="username"]', 'admin');
        await page.fill('input[name="password"]', 'admin');
        await page.click('input[type="submit"]');
        
        // Wait for successful login and redirect back to our app
        console.log('7️⃣ Waiting for successful login...');
        await page.waitForURL('http://localhost:5000/', { timeout: 15000 });
        console.log('✅ Successfully logged in after F5 refresh test');
        
        console.log('🎉 F5 refresh test completed successfully!');
        return true;
        
    } catch (error) {
        console.error('❌ F5 refresh test failed:', error.message);
        
        // Take a screenshot for debugging
        try {
            await page.screenshot({ path: 'f5-refresh-error.png' });
            console.log('📸 Screenshot saved as f5-refresh-error.png');
        } catch (screenshotError) {
            console.log('⚠️ Could not take screenshot:', screenshotError.message);
        }
        
        return false;
    } finally {
        await browser.close();
    }
}

// Additional test for multiple F5 refreshes
async function testMultipleF5Refreshes() {
    console.log('🔄 Testing multiple F5 refreshes on login screen...');
    
    const browser = await chromium.launch({ 
        headless: false,
        slowMo: 500
    });
    
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Navigate to login
        await page.goto('http://localhost:5000/auth/login');
        await page.waitForURL('**/auth/realms/systeminstaller/protocol/openid-connect/auth**', { timeout: 10000 });
        
        // Perform multiple F5 refreshes
        for (let i = 1; i <= 3; i++) {
            console.log(`${i}️⃣ F5 refresh #${i}...`);
            await page.reload();
            await page.waitForTimeout(2000);
            
            const currentUrl = page.url();
            console.log(`📍 URL after refresh #${i}:`, currentUrl);
            
            // Check that we don't get stuck in an error state
            const pageText = await page.textContent('body');
            if (pageText.includes('invalid_request')) {
                console.log(`❌ Found invalid_request error after refresh #${i}`);
                return false;
            }
        }
        
        console.log('✅ Multiple F5 refreshes handled successfully');
        return true;
        
    } catch (error) {
        console.error('❌ Multiple F5 refresh test failed:', error.message);
        return false;
    } finally {
        await browser.close();
    }
}

// Test F5 refresh at different stages of the login flow
async function testF5RefreshAtDifferentStages() {
    console.log('🔄 Testing F5 refresh at different stages of login flow...');
    
    const browser = await chromium.launch({ 
        headless: false,
        slowMo: 1000
    });
    
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Test 1: F5 refresh immediately after clicking login
        console.log('1️⃣ Testing F5 refresh right after login redirect...');
        await page.goto('http://localhost:5000');
        await page.waitForURL('**/auth/realms/systeminstaller/protocol/openid-connect/auth**', { timeout: 10000 });
        
        // Refresh immediately
        await page.reload();
        await page.waitForTimeout(2000);
        
        // Test 2: F5 refresh after entering username but before password
        console.log('2️⃣ Testing F5 refresh after entering username...');
        await page.goto('http://localhost:5000/auth/login');
        await page.waitForURL('**/auth/realms/systeminstaller/protocol/openid-connect/auth**', { timeout: 10000 });
        
        await page.fill('input[name="username"]', 'admin');
        await page.reload();
        await page.waitForTimeout(2000);
        
        // Test 3: F5 refresh after entering both username and password but before submit
        console.log('3️⃣ Testing F5 refresh after entering credentials...');
        await page.goto('http://localhost:5000/auth/login');
        await page.waitForURL('**/auth/realms/systeminstaller/protocol/openid-connect/auth**', { timeout: 10000 });
        
        await page.fill('input[name="username"]', 'admin');
        await page.fill('input[name="password"]', 'admin');
        await page.reload();
        await page.waitForTimeout(2000);
        
        console.log('✅ F5 refresh at different stages completed successfully');
        return true;
        
    } catch (error) {
        console.error('❌ F5 refresh at different stages test failed:', error.message);
        return false;
    } finally {
        await browser.close();
    }
}

// Run all F5 refresh tests
async function runAllF5Tests() {
    console.log('🚀 Starting comprehensive F5 refresh tests...');
    
    const results = {
        basicF5Test: await testF5RefreshOnLogin(),
        multipleF5Test: await testMultipleF5Refreshes(),
        stagesF5Test: await testF5RefreshAtDifferentStages()
    };
    
    console.log('\n📊 Test Results:');
    console.log('Basic F5 refresh test:', results.basicF5Test ? '✅ PASSED' : '❌ FAILED');
    console.log('Multiple F5 refresh test:', results.multipleF5Test ? '✅ PASSED' : '❌ FAILED');
    console.log('F5 at different stages test:', results.stagesF5Test ? '✅ PASSED' : '❌ FAILED');
    
    const allPassed = Object.values(results).every(result => result === true);
    
    if (allPassed) {
        console.log('\n🎉 All F5 refresh tests PASSED! The fix is working correctly.');
    } else {
        console.log('\n❌ Some F5 refresh tests FAILED. The fix may need adjustments.');
    }
    
    return allPassed;
}

// Run the tests
if (require.main === module) {
    runAllF5Tests().catch(console.error);
}

module.exports = { testF5RefreshOnLogin, testMultipleF5Refreshes, testF5RefreshAtDifferentStages, runAllF5Tests };
