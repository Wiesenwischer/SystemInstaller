const { chromium } = require('playwright');

async function testLogoutRedirect() {
  console.log('🚀 Testing logout redirect behavior...');
  
  const browser = await chromium.launch({ 
    headless: true,  // Run headless to avoid browser hanging
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });
  
  const page = await browser.newPage();
  
  // Add response listener to track redirects
  const responses = [];
  page.on('response', response => {
    responses.push({
      url: response.url(),
      status: response.status(),
      headers: response.headers()
    });
    console.log(`📥 Response: ${response.status()} ${response.url()}`);
  });
  
  try {
    console.log('🚪 Navigating to logout endpoint...');
    
    // Navigate to logout endpoint
    const response = await page.goto('http://localhost:5000/auth/logout', {
      waitUntil: 'networkidle',
      timeout: 10000
    });
    
    console.log(`📍 Final URL: ${page.url()}`);
    console.log(`📊 Response status: ${response.status()}`);
    
    // Check if we're on Keycloak
    const finalUrl = page.url();
    const isKeycloak = finalUrl.includes('keycloak') || 
                      finalUrl.includes('host.docker.internal:8082') ||
                      finalUrl.includes('realms/systeminstaller');
    
    console.log('\n📋 Response Chain:');
    responses.forEach((resp, i) => {
      console.log(`  ${i + 1}. ${resp.status} ${resp.url}`);
      if (resp.headers.location) {
        console.log(`     → Location: ${resp.headers.location}`);
      }
    });
    
    if (isKeycloak) {
      console.log('\n✅ SUCCESS: Logout correctly redirected to Keycloak!');
      return true;
    } else {
      console.log('\n❌ FAILURE: Logout did not redirect to Keycloak');
      console.log(`   Expected: URL containing 'keycloak' or 'host.docker.internal:8082'`);
      console.log(`   Actual: ${finalUrl}`);
      return false;
    }
    
  } catch (error) {
    console.error('❌ Error during logout test:', error.message);
    return false;
  } finally {
    await browser.close();
  }
}

// Run the test
testLogoutRedirect().then(success => {
  if (success) {
    console.log('\n🎉 LOGOUT REDIRECT TEST PASSED! 🎉');
    process.exit(0);
  } else {
    console.log('\n💥 LOGOUT REDIRECT TEST FAILED! 💥');
    process.exit(1);
  }
});
