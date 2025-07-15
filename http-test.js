const http = require('http');

function makeRequest(url, method = 'GET') {
    return new Promise((resolve, reject) => {
        const urlObj = new URL(url);
        const options = {
            hostname: urlObj.hostname,
            port: urlObj.port,
            path: urlObj.pathname,
            method: method,
            headers: {
                'User-Agent': 'Test-Agent/1.0'
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
                    body: data,
                    url: url
                });
            });
        });

        req.on('error', (error) => {
            reject(error);
        });

        req.end();
    });
}

async function testEndpoints() {
    console.log('🧪 Testing HTTP endpoints...');
    
    try {
        // Test health endpoint
        console.log('\n📍 Testing health endpoint:');
        const healthResponse = await makeRequest('http://localhost:5000/health');
        console.log(`Status: ${healthResponse.status}`);
        console.log(`Body: ${healthResponse.body}`);
        
        // Test root endpoint (should redirect to auth)
        console.log('\n📍 Testing root endpoint:');
        const rootResponse = await makeRequest('http://localhost:5000/');
        console.log(`Status: ${rootResponse.status}`);
        console.log(`Headers: ${JSON.stringify(rootResponse.headers, null, 2)}`);
        
        // Test logout endpoint
        console.log('\n📍 Testing logout endpoint:');
        const logoutResponse = await makeRequest('http://localhost:5000/logout');
        console.log(`Status: ${logoutResponse.status}`);
        console.log(`Headers: ${JSON.stringify(logoutResponse.headers, null, 2)}`);
        
        console.log('\n✅ All tests completed');
        
    } catch (error) {
        console.error('❌ Error:', error.message);
    }
}

testEndpoints();
