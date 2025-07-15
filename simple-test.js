const http = require('http');

console.log('Testing health endpoint...');

const req = http.request({
  hostname: 'localhost',
  port: 5000,
  path: '/health',
  method: 'GET'
}, (res) => {
  console.log(`Health endpoint status: ${res.statusCode}`);
  
  let data = '';
  res.on('data', chunk => data += chunk);
  res.on('end', () => {
    console.log(`Health response: ${data}`);
    
    // Now test logout endpoint
    console.log('\nTesting logout endpoint...');
    
    const logoutReq = http.request({
      hostname: 'localhost',
      port: 5000,
      path: '/auth/logout',
      method: 'GET'
    }, (logoutRes) => {
      console.log(`Logout endpoint status: ${logoutRes.statusCode}`);
      console.log(`Logout headers:`, logoutRes.headers);
      
      let logoutData = '';
      logoutRes.on('data', chunk => logoutData += chunk);
      logoutRes.on('end', () => {
        console.log(`Logout response: ${logoutData}`);
        
        if (logoutRes.statusCode === 302 && logoutRes.headers.location === '/') {
          console.log('\n✅ LOGOUT WORKING! Redirects to root as expected.');
        } else if (logoutRes.statusCode === 302) {
          console.log(`\n⚠️  Logout redirects to: ${logoutRes.headers.location}`);
        } else {
          console.log(`\n❌ Unexpected logout response: ${logoutRes.statusCode}`);
        }
      });
    });
    
    logoutReq.on('error', (error) => {
      console.log(`Logout request error: ${error.message}`);
    });
    
    logoutReq.end();
  });
});

req.on('error', (error) => {
  console.log(`Health request error: ${error.message}`);
});

req.end();
