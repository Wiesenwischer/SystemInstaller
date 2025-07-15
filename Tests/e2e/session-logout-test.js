const { chromium } = require('playwright');

async function testSessionLogout() {
  try {
    console.log('🔍 Testing COMPLETE logout with session termination...');
    
    const browser = await chromium.launch({ 
      headless: false,
      slowMo: 1000,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    const page = await browser.newPage();
    
    // 1. Initial login
    console.log('🔑 Step 1: Logging in...');
    await page.goto('http://localhost:5000');
    await page.waitForURL('**/realms/systeminstaller/**', { timeout: 10000 });
    await page.fill('#username', 'admin');
    await page.fill('#password', 'admin123');
    await page.click('input[type="submit"]');
    await page.waitForURL('http://localhost:5000/', { timeout: 10000 });
    console.log('✅ Login successful');
    
    // 2. Test that protected endpoint is accessible
    console.log('🔒 Step 2: Testing protected endpoint access...');
    const protectedResponse = await page.goto('http://localhost:5000/auth/user');
    if (protectedResponse.status() === 200) {
      console.log('✅ Protected endpoint accessible while logged in');
    } else {
      console.log('❌ Protected endpoint not accessible - unexpected!');
      await browser.close();
      return;
    }
    
    // 3. Perform logout
    console.log('🚪 Step 3: Performing logout...');
    await page.goto('http://localhost:5000/auth/logout');
    
    // Wait for Keycloak logout to complete
    await page.waitForTimeout(3000);
    
    // 4. Test if session is really terminated
    console.log('🔍 Step 4: Testing if session is terminated...');
    try {
      const testResponse = await page.goto('http://localhost:5000/auth/user');
      const status = testResponse.status();
      
      if (status === 401) {
        console.log('✅ SUCCESS! Session terminated - protected endpoint returns 401');
      } else if (status === 302) {
        console.log('⚠️  Protected endpoint redirects (302) - checking redirect target...');
        const finalUrl = page.url();
        if (finalUrl.includes('keycloak') || finalUrl.includes('auth')) {
          console.log('✅ SUCCESS! Redirected to login - session terminated');
        } else {
          console.log('❌ FAIL! Redirected but not to login page');
        }
      } else {
        console.log(`❌ FAIL! Protected endpoint returns ${status} - session NOT terminated`);
      }
    } catch (error) {
      console.log('⚠️  Error testing protected endpoint:', error.message);
    }
    
    // 5. The critical F5 test
    console.log('🔄 Step 5: THE CRITICAL F5 TEST...');
    await page.goto('http://localhost:5000');
    await page.waitForTimeout(2000);
    
    const currentUrl = page.url();
    if (currentUrl.includes('keycloak') || currentUrl.includes('auth')) {
      console.log('✅ SUCCESS! F5 requires re-authentication - bug FIXED!');
    } else if (currentUrl === 'http://localhost:5000/') {
      console.log('❌ BUG STILL EXISTS! F5 automatically logs user back in');
    } else {
      console.log(`⚠️  Unexpected URL after F5: ${currentUrl}`);
    }
    
    console.log('👀 Look at the browser to confirm what you see...');
    await page.waitForTimeout(5000);
    
    await browser.close();
    console.log('✅ Test completed!');
    
  } catch (error) {
    console.error('❌ Error:', error.message);
  }
}

testSessionLogout();
