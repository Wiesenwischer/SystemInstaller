const { chromium } = require('playwright');

async function detailedYarpAnalysis() {
  console.log('🔧 Detailed YARP Analysis - What is really happening?');
  
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  try {
    console.log('\n=== TESTING /auth/logout IN DETAIL ===');
    
    const response = await page.request.get('http://localhost:5000/auth/logout');
    const status = response.status();
    const headers = response.headers();
    const body = await response.text();
    
    console.log(`Status: ${status}`);
    console.log(`Headers:`, JSON.stringify(headers, null, 2));
    console.log(`Body length: ${body.length}`);
    console.log(`Body preview (first 500 chars):`);
    console.log(body.substring(0, 500));
    console.log('...');
    
    // Check specific indicators
    if (body.includes('Cannot GET /auth/logout')) {
      console.log('\n❌ PROBLEM DETECTED: Frontend 404 - "Cannot GET /auth/logout"');
    } else if (body.includes('<!DOCTYPE html>') && body.includes('react')) {
      console.log('\n❌ PROBLEM DETECTED: Getting React app HTML instead of logout handler');
    } else if (body.includes('Keycloak') || headers['location']) {
      console.log('\n✅ GOOD: Logout handler working - redirecting to Keycloak');
    } else {
      console.log('\n⚠️  UNCLEAR: Need to investigate response content');
    }
    
    console.log('\n=== TESTING /auth/user IN DETAIL ===');
    
    const userResponse = await page.request.get('http://localhost:5000/auth/user');
    const userStatus = userResponse.status();
    const userHeaders = userResponse.headers();
    const userBody = await userResponse.text();
    
    console.log(`Status: ${userStatus}`);
    console.log(`Headers:`, JSON.stringify(userHeaders, null, 2));
    console.log(`Body preview (first 200 chars):`);
    console.log(userBody.substring(0, 200));
    
    if (userBody.includes('Cannot GET /auth/user')) {
      console.log('\n❌ PROBLEM: /auth/user being proxied to frontend');
    } else if (userStatus === 401 || userStatus === 403) {
      console.log('\n✅ GOOD: /auth/user correctly returning auth challenge');
    } else {
      console.log('\n⚠️  UNCLEAR: Unexpected auth/user response');
    }
    
  } finally {
    await browser.close();
  }
}

detailedYarpAnalysis().catch(console.error);
