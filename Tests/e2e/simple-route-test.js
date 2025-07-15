const http = require('http');

async function testRoutes() {
  console.log('🧪 Testing Gateway Routes');
  console.log('========================');
  
  const routes = [
    { path: '/health', description: 'Health endpoint (should return JSON)' },
    { path: '/gateway/info', description: 'Gateway info (should return JSON)' },
    { path: '/auth/logout', description: 'Logout endpoint (should redirect or return response)' }
  ];
  
  for (const route of routes) {
    console.log(`\n📍 Testing ${route.path}: ${route.description}`);
    
    try {
      await new Promise((resolve, reject) => {
        const req = http.request({
          hostname: 'localhost',
          port: 5000,
          path: route.path,
          method: 'GET'
        }, (res) => {
          console.log(`   Status: ${res.statusCode}`);
          console.log(`   Headers: ${JSON.stringify(res.headers, null, 2)}`);
          
          let data = '';
          res.on('data', chunk => data += chunk);
          res.on('end', () => {
            if (res.headers['content-type']?.includes('application/json')) {
              try {
                const json = JSON.parse(data);
                console.log(`   ✅ JSON Response: ${JSON.stringify(json, null, 2)}`);
              } catch (e) {
                console.log(`   ⚠️  Invalid JSON: ${data}`);
              }
            } else if (res.statusCode >= 300 && res.statusCode < 400) {
              console.log(`   🔄 Redirect to: ${res.headers.location}`);
            } else {
              console.log(`   📄 Content (first 200 chars): ${data.substring(0, 200)}`);
            }
            
            // Check for specific issues
            if (route.path === '/health' && res.statusCode !== 200) {
              console.log(`   ❌ PROBLEM: Health endpoint should return 200, got ${res.statusCode}`);
            }
            
            if (route.path === '/auth/logout') {
              if (res.statusCode === 404) {
                console.log(`   ❌ PROBLEM: Logout returning 404 - routing issue!`);
              } else if (res.statusCode === 302 && res.headers.location?.includes('keycloak')) {
                console.log(`   ✅ GOOD: Logout redirecting to Keycloak`);
              } else {
                console.log(`   ⚠️  Unexpected logout behavior`);
              }
            }
            
            resolve();
          });
        });
        
        req.on('error', (error) => {
          console.log(`   ❌ Error: ${error.message}`);
          resolve();
        });
        
        req.setTimeout(10000, () => {
          req.destroy();
          console.log(`   ❌ Timeout`);
          resolve();
        });
        
        req.end();
      });
    } catch (error) {
      console.log(`   ❌ Exception: ${error.message}`);
    }
  }
  
  console.log('\n🎯 SUMMARY');
  console.log('==========');
  console.log('✅ Health endpoint should return 200 with JSON');
  console.log('✅ Logout should either redirect to Keycloak or return proper response');
  console.log('❌ If logout returns 404, the routing is still broken');
}

testRoutes().catch(console.error);
