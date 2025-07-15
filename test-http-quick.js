// Quick HTTP test to verify authentication flow
const http = require('http');

async function quickHttpTest() {
  console.log('🚀 Quick HTTP test of gateway endpoints...');
  
  return new Promise((resolve) => {
    // Test health endpoint first
    const healthReq = http.request('http://localhost:5000/health', (res) => {
      console.log(`✅ Health endpoint: ${res.statusCode}`);
      
      // Test protected endpoint
      const protectedReq = http.request('http://localhost:5000/api/protected', (res2) => {
        console.log(`🔐 Protected endpoint: ${res2.statusCode}`);
        console.log(`📍 Redirect location: ${res2.headers.location || 'None'}`);
        
        // Test logout endpoint
        const logoutReq = http.request('http://localhost:5000/auth/logout', (res3) => {
          console.log(`🚪 Logout endpoint: ${res3.statusCode}`);
          console.log(`📍 Logout redirect: ${res3.headers.location || 'None'}`);
          resolve();
        });
        
        logoutReq.on('error', (err) => {
          console.log(`❌ Logout error: ${err.message}`);
          resolve();
        });
        
        logoutReq.end();
      });
      
      protectedReq.on('error', (err) => {
        console.log(`❌ Protected endpoint error: ${err.message}`);
        resolve();
      });
      
      protectedReq.end();
    });
    
    healthReq.on('error', (err) => {
      console.log(`❌ Health endpoint error: ${err.message}`);
      resolve();
    });
    
    healthReq.end();
  });
}

quickHttpTest().then(() => {
  console.log('✅ HTTP test completed');
});
