const https = require('https');
const http = require('http');

async function httpTest(path) {
  return new Promise((resolve, reject) => {
    const options = {
      hostname: 'localhost',
      port: 5000,
      path: path,
      method: 'GET',
      headers: {
        'User-Agent': 'Test-Client/1.0'
      }
    };

    const req = http.request(options, (res) => {
      let data = '';
      
      res.on('data', (chunk) => {
        data += chunk;
      });
      
      res.on('end', () => {
        resolve({
          status: res.statusCode,
          headers: res.headers,
          data: data,
          url: res.headers.location || path
        });
      });
    });

    req.on('error', (err) => {
      reject(err);
    });

    req.setTimeout(10000, () => {
      req.destroy();
      reject(new Error('Request timeout'));
    });

    req.end();
  });
}

async function simpleRoutingTest() {
  console.log('🔍 Simple HTTP Routing Test');
  console.log('============================');
  
  const routes = [
    '/health',
    '/gateway/info', 
    '/auth/user',
    '/auth/logout',
    '/'
  ];
  
  for (const route of routes) {
    try {
      console.log(`\n📍 Testing ${route}:`);
      const result = await httpTest(route);
      
      console.log(`   Status: ${result.status}`);
      console.log(`   Content-Type: ${result.headers['content-type'] || 'unknown'}`);
      
      if (result.headers.location) {
        console.log(`   Redirect: ${result.headers.location}`);
      }
      
      // Check for YARP routing issues
      const contentType = result.headers['content-type'] || '';
      const isApiRoute = route.startsWith('/auth') || route.startsWith('/api') || route === '/health' || route === '/gateway/info';
      
      if (isApiRoute && contentType.includes('text/html')) {
        console.log('   ⚠️  WARNING: API route returning HTML - likely YARP proxied!');
        
        // Check if it's Keycloak HTML
        if (result.data.includes('Keycloak') || result.data.includes('kc-')) {
          console.log('   📄 Content: Keycloak login page HTML');
        } else if (result.data.includes('404') || result.data.includes('Not Found')) {
          console.log('   ❌ Content: 404 Error page');
        } else {
          console.log(`   📄 Content preview: ${result.data.substring(0, 100)}...`);
        }
      } else if (contentType.includes('application/json')) {
        try {
          const jsonData = JSON.parse(result.data);
          console.log(`   ✅ JSON Response: ${JSON.stringify(jsonData, null, 2)}`);
        } catch (e) {
          console.log('   ⚠️  Invalid JSON response');
        }
      }
      
    } catch (error) {
      console.log(`\n📍 Testing ${route}:`);
      console.log(`   ❌ Error: ${error.message}`);
    }
  }
  
  console.log('\n🎯 ANALYSIS COMPLETE');
  console.log('===================');
  console.log('• Look for "WARNING: API route returning HTML" - these indicate YARP routing problems');
  console.log('• /health and /gateway/info should return JSON');
  console.log('• /auth routes should return proper redirects or 401, not HTML content');
}

simpleRoutingTest().catch(console.error);
