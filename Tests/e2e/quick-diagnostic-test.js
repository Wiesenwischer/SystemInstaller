const { chromium } = require('playwright');

async function quickDiagnosticTest() {
  console.log('🔍 Quick YARP Routing Diagnostic Test');
  console.log('===================================');
  
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  try {
    // Test critical routes
    const routeTests = [
      { path: '/health', expected: 'JSON health status' },
      { path: '/auth/logout', expected: 'Redirect to Keycloak or proper logout' },
      { path: '/auth/user', expected: '401 or redirect to login' },
      { path: '/', expected: 'Frontend HTML or redirect to login' }
    ];
    
    console.log('\n🧪 Testing Route Handling:');
    
    for (const test of routeTests) {
      try {
        const response = await page.goto(`http://localhost:5000${test.path}`, { 
          waitUntil: 'networkidle',
          timeout: 8000 
        });
        
        const status = response.status();
        const contentType = response.headers()['content-type'] || 'unknown';
        const finalUrl = response.url();
        
        console.log(`\n📍 ${test.path}:`);
        console.log(`   Status: ${status}`);
        console.log(`   Content-Type: ${contentType}`);
        console.log(`   Final URL: ${finalUrl}`);
        console.log(`   Expected: ${test.expected}`);
        
        // Quick content check
        if (contentType.includes('text/html')) {
          const title = await page.title();
          const bodyText = await page.textContent('body');
          console.log(`   Page Title: ${title}`);
          
          if (bodyText.includes('Keycloak')) {
            console.log('   ✅ Keycloak content detected');
          } else if (bodyText.includes('404') || title.includes('404')) {
            console.log('   ❌ 404 ERROR DETECTED!');
          } else if (bodyText.length < 100) {
            console.log(`   📄 Short content: ${bodyText.trim()}`);
          }
        } else if (contentType.includes('application/json')) {
          try {
            const jsonData = await response.json();
            console.log(`   📄 JSON: ${JSON.stringify(jsonData, null, 2)}`);
          } catch (e) {
            console.log('   ⚠️  Failed to parse JSON');
          }
        }
        
        // Check if route appears to be handled by YARP vs Gateway API
        if (test.path.startsWith('/auth') || test.path.startsWith('/api') || test.path === '/health') {
          if (contentType.includes('text/html') && finalUrl.includes('localhost:5000')) {
            console.log('   ⚠️  WARNING: API route returning HTML - likely YARP proxied to frontend!');
          }
        }
        
      } catch (error) {
        console.log(`\n📍 ${test.path}:`);
        console.log(`   ❌ Error: ${error.message}`);
      }
    }
    
    console.log('\n🎯 DIAGNOSIS COMPLETE');
    console.log('============================');
    console.log('Look for routes marked with "WARNING: API route returning HTML"');
    console.log('These indicate YARP is incorrectly proxying auth/api routes to the frontend.');
    
  } catch (error) {
    console.error('❌ Test failed:', error.message);
  } finally {
    await browser.close();
  }
}

quickDiagnosticTest().catch(console.error);
