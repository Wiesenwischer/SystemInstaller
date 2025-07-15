const { chromium } = require('playwright');

async function runQuickYarpTest() {
  console.log('🔧 Quick YARP Routing Test (Headless)');
  
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  try {
    // Test 1: Health check
    console.log('\n🔍 Testing Gateway Health...');
    const healthResponse = await page.request.get('http://localhost:5000/health');
    console.log(`Health status: ${healthResponse.status()}`);
    
    // Test 2: Auth routes
    console.log('\n🔍 Testing Auth Routes...');
    const authRoutes = ['/auth/logout', '/auth/login', '/auth/user', '/api/protected'];
    
    for (const route of authRoutes) {
      try {
        const response = await page.request.get(`http://localhost:5000${route}`);
        const status = response.status();
        const headers = response.headers();
        const contentType = headers['content-type'] || '';
        const server = headers['server'] || '';
        
        console.log(`\n${route}:`);
        console.log(`  Status: ${status}`);
        console.log(`  Server: ${server}`);
        console.log(`  Content-Type: ${contentType}`);
        
        // Check if this looks like a frontend 404
        if (status === 404 && !server.includes('Kestrel')) {
          const body = await response.text();
          if (body.includes('Cannot GET') || body.includes('404')) {
            console.log(`  ❌ PROBLEM: ${route} proxied to frontend!`);
            console.log(`  Body preview: ${body.substring(0, 100)}...`);
          }
        } else if ([200, 302, 401, 403].includes(status)) {
          console.log(`  ✅ Correctly handled by Gateway`);
        } else {
          console.log(`  ⚠️  Unexpected status: ${status}`);
        }
      } catch (error) {
        console.log(`  ❌ Error: ${error.message}`);
      }
    }
    
    // Test 3: Frontend routes
    console.log('\n🔍 Testing Frontend Routes...');
    const frontendRoutes = ['/', '/environments', '/installations'];
    
    for (const route of frontendRoutes) {
      try {
        const response = await page.request.get(`http://localhost:5000${route}`);
        const status = response.status();
        const headers = response.headers();
        const contentType = headers['content-type'] || '';
        
        console.log(`\n${route}:`);
        console.log(`  Status: ${status}`);
        console.log(`  Content-Type: ${contentType}`);
        
        if (status === 200 && contentType.includes('text/html')) {
          console.log(`  ✅ Correctly proxied to frontend`);
        } else if (status === 302) {
          console.log(`  ✅ Redirected (likely to auth)`);
        } else {
          console.log(`  ⚠️  Unexpected response`);
        }
      } catch (error) {
        console.log(`  ❌ Error: ${error.message}`);
      }
    }
    
  } finally {
    await browser.close();
  }
  
  console.log('\n✅ Quick YARP test completed!');
}

runQuickYarpTest().catch(console.error);
