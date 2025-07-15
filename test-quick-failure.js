const { chromium } = require('playwright');

async function quickFailureTest() {
  console.log('🔍 Quick test to diagnose the authentication failure...');
  
  const browser = await chromium.launch({ 
    headless: false,
    slowMo: 1000
  });
  
  const page = await browser.newPage();
  
  // Add detailed logging
  page.on('response', response => {
    if (response.status() >= 400) {
      console.log(`❌ ERROR: ${response.status()} ${response.url()}`);
    } else if (response.url().includes('signin-oidc') || response.url().includes('protected')) {
      console.log(`📍 AUTH: ${response.status()} ${response.url()}`);
    }
  });
  
  try {
    console.log('🔐 Testing protected endpoint directly...');
    await page.goto('http://localhost:5000/api/protected');
    
    // Wait for any redirects or errors
    await page.waitForTimeout(5000);
    
    const currentUrl = page.url();
    const title = await page.title();
    const bodyText = await page.locator('body').textContent();
    
    console.log(`📍 Final URL: ${currentUrl}`);
    console.log(`📝 Page title: ${title}`);
    
    if (bodyText.includes('AuthenticationFailureException') || 
        bodyText.includes('Correlation failed') ||
        bodyText.includes('Exception')) {
      console.log('❌ CORRELATION ERROR DETECTED!');
      const lines = bodyText.split('\n');
      for (const line of lines.slice(0, 10)) {
        if (line.trim().length > 0) {
          console.log(`   ${line.trim()}`);
        }
      }
    } else if (currentUrl.includes('keycloak')) {
      console.log('✅ Properly redirected to Keycloak');
    } else {
      console.log('📄 Body preview:', bodyText.substring(0, 200));
    }
    
  } catch (error) {
    console.error('❌ Error:', error.message);
  } finally {
    await browser.close();
  }
}

quickFailureTest();
