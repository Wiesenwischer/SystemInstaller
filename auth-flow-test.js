const { chromium } = require('playwright');

(async () => {
    console.log('🔐 Testing authentication flow...');
    
    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext({
        ignoreHTTPSErrors: true
    });
    
    const page = await context.newPage();
    
    try {
        console.log('📍 Step 1: Navigate to gateway');
        await page.goto('http://localhost:5000');
        await page.waitForTimeout(2000);
        
        const currentUrl = page.url();
        console.log('Current URL:', currentUrl);
        
        if (currentUrl.includes('keycloak') || currentUrl.includes('host.docker.internal:8082') || currentUrl.includes('openid-connect/auth')) {
            console.log('✅ Redirected to Keycloak login - authentication working');
            
            // Try to login
            console.log('📍 Step 2: Attempting login');
            
            // Wait for login form to be visible
            await page.waitForSelector('#username', { timeout: 5000 });
            await page.waitForSelector('#password', { timeout: 5000 });
            
            console.log('📝 Filling in username...');
            await page.fill('#username', 'admin');
            
            console.log('📝 Filling in password...');
            await page.fill('#password', 'admin123');
            
            console.log('🔘 Clicking login button...');
            
            // First, let's see what's on the page
            const pageContent = await page.content();
            console.log('Page contains "Sign in" text:', pageContent.includes('Sign in'));
            console.log('Page contains "Login" text:', pageContent.includes('Login'));
            
            // Try multiple selectors for the login button
            const loginButtonSelectors = [
                'input[type="submit"]',
                'button[type="submit"]',
                '#kc-login',
                'input[name="login"]',
                'button[name="login"]',
                '.btn-primary',
                'input[value="Sign In"]',
                'button:contains("Sign In")',
                'input[id="kc-login"]',
                'button[id="kc-login"]',
                '.kc-button-primary'
            ];
            
            let buttonClicked = false;
            for (const selector of loginButtonSelectors) {
                try {
                    const element = await page.waitForSelector(selector, { timeout: 1000 });
                    if (element) {
                        await page.click(selector);
                        console.log(`✅ Clicked login button with selector: ${selector}`);
                        buttonClicked = true;
                        break;
                    }
                } catch (error) {
                    console.log(`⚠️ Selector ${selector} not found, trying next...`);
                }
            }
            
            if (!buttonClicked) {
                console.log('❌ Could not find login button with any selector');
                // Let's try to find any button or input element
                const buttons = await page.$$('button, input[type="submit"], input[type="button"]');
                console.log(`Found ${buttons.length} buttons/inputs on the page`);
                
                if (buttons.length > 0) {
                    console.log('Trying to click the first button...');
                    await buttons[0].click();
                    buttonClicked = true;
                } else {
                    console.log('No buttons found, trying Enter key...');
                    await page.keyboard.press('Enter');
                    buttonClicked = true;
                }
            }
            
            console.log('⏳ Waiting for login to complete...');
            await page.waitForTimeout(5000);
            
            const postLoginUrl = page.url();
            console.log('Post-login URL:', postLoginUrl);
            
            if (postLoginUrl.includes('localhost:5000')) {
                console.log('✅ Successfully logged in and redirected back to application');
                
                // Test logout
                console.log('📍 Step 3: Testing logout');
                
                // First, let's see what buttons are available on the page
                const allButtons = await page.$$('button');
                console.log(`Found ${allButtons.length} buttons on the page`);
                
                for (let i = 0; i < allButtons.length; i++) {
                    const buttonText = await allButtons[i].textContent();
                    console.log(`Button ${i}: "${buttonText}"`);
                }
                
                // First try to find and click the user dropdown (admin button)
                try {
                    console.log('🔍 Looking for user dropdown button...');
                    
                    // Try different selectors for the user dropdown
                    const dropdownSelectors = [
                        'button:has-text("admin")',
                        'button:has-text("Benutzer")',
                        '.dropdown-toggle',
                        'button[data-bs-toggle="dropdown"]',
                        'button.dropdown-toggle'
                    ];
                    
                    let dropdownClicked = false;
                    for (const selector of dropdownSelectors) {
                        try {
                            await page.click(selector);
                            console.log(`✅ Clicked dropdown button with selector: ${selector}`);
                            dropdownClicked = true;
                            await page.waitForTimeout(1000); // Wait for dropdown to open
                            break;
                        } catch (e) {
                            console.log(`⚠️ Dropdown selector ${selector} not found, trying next...`);
                        }
                    }
                    
                    if (!dropdownClicked) {
                        console.log('⚠️ Could not find dropdown button, trying direct logout button');
                        throw new Error('Dropdown not found');
                    }
                    
                    // Now look for the "Sign out" menu item in the dropdown
                    console.log('🔍 Looking for Sign out menu item in dropdown...');
                    const signOutSelectors = [
                        'a:has-text("Sign out")',
                        'button:has-text("Sign out")',
                        '.dropdown-item:has-text("Sign out")',
                        'a:has-text("Abmelden")',
                        'button:has-text("Abmelden")',
                        '.dropdown-item:has-text("Abmelden")',
                        'a:has-text("Logout")',
                        'button:has-text("Logout")',
                        '.dropdown-item:has-text("Logout")'
                    ];
                    
                    let signOutClicked = false;
                    for (const selector of signOutSelectors) {
                        try {
                            await page.click(selector);
                            console.log(`✅ Clicked sign out with selector: ${selector}`);
                            signOutClicked = true;
                            await page.waitForTimeout(2000);
                            break;
                        } catch (e) {
                            console.log(`⚠️ Sign out selector ${selector} not found, trying next...`);
                        }
                    }
                    
                    if (!signOutClicked) {
                        throw new Error('Sign out menu item not found');
                    }
                    
                } catch (error) {
                    console.log('⚠️ Could not find dropdown or sign out menu, trying alternative approach');
                    
                    // Fallback: Try to find the logout button directly
                    const logoutSelectors = [
                        'button:has-text("Abmelden")',
                        'form[action="/auth/logout"] button',
                        '.btn-outline-dark'
                    ];
                    
                    let buttonFound = false;
                    for (const selector of logoutSelectors) {
                        try {
                            await page.click(selector);
                            console.log(`✅ Clicked logout button with selector: ${selector}`);
                            buttonFound = true;
                            break;
                        } catch (e) {
                            console.log(`⚠️ Logout selector ${selector} not found, trying next...`);
                        }
                    }
                    
                    if (!buttonFound) {
                        console.log('⚠️ Could not find logout button, trying direct URL');
                        const logoutResponse = await page.goto('http://localhost:5000/auth/logout');
                        console.log('Logout status:', logoutResponse.status());
                    }
                }
                
                const logoutUrl = page.url();
                console.log('Logout URL:', logoutUrl);
                
                if (logoutUrl.includes('keycloak') || logoutUrl.includes('host.docker.internal:8082') || logoutUrl.includes('openid-connect/auth')) {
                    console.log('✅ Logout successful - redirected to Keycloak login (user is now logged out)');
                } else if (logoutUrl === 'http://localhost:5000/') {
                    console.log('✅ Logout successful - stayed on application root');
                } else {
                    console.log('⚠️ Logout status not as expected');
                    console.log('Current URL after logout:', logoutUrl);
                }
            } else {
                console.log('❌ Login failed - still on login page');
            }
        } else {
            console.log('❌ Not redirected to Keycloak - authentication may not be working');
        }
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
    } finally {
        await browser.close();
        console.log('🏁 Test completed');
    }
})();
