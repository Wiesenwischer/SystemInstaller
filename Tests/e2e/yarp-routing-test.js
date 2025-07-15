const { chromium } = require('playwright');

class YarpRoutingTest {
  constructor() {
    this.browser = null;
    this.page = null;
    this.testResults = {
      steps: [],
      scenarios: [],
      routingResults: {},
      authRouteResults: {},
      frontendRouteResults: {}
    };
  }

  log(message) {
    console.log(message);
    this.testResults.steps.push(`${new Date().toISOString()}: ${message}`);
  }

  async setup() {
    this.log('🔧 Setting up YARP Routing Test Environment');
    this.browser = await chromium.launch({ 
      headless: false, 
      slowMo: 1000 
    });
    this.page = await this.browser.newPage();
    this.log('✅ Browser setup completed');
  }

  async teardown() {
    this.log('🧹 Cleaning up test environment');
    if (this.browser) {
      await this.browser.close();
    }
    
    this.log('');
    this.log('📊 YARP ROUTING TEST SUMMARY');
    this.log('============================');
    this.log(`Total scenarios tested: ${this.testResults.scenarios.length}`);
    
    const authRoutesWorking = Object.values(this.testResults.authRouteResults).every(r => r.success);
    const frontendRoutesWorking = Object.values(this.testResults.frontendRouteResults).every(r => r.success);
    
    this.log('');
    this.log('🔐 Auth Routes Status:');
    if (authRoutesWorking) {
      this.log('✅ All auth routes are correctly handled by Gateway (not proxied)');
    } else {
      this.log('❌ Some auth routes are being incorrectly proxied to frontend');
      Object.entries(this.testResults.authRouteResults).forEach(([route, result]) => {
        this.log(`   ${route}: ${result.success ? '✅' : '❌'} - ${result.description}`);
      });
    }
    
    this.log('');
    this.log('🌐 Frontend Routes Status:');
    if (frontendRoutesWorking) {
      this.log('✅ All frontend routes are correctly proxied to web container');
    } else {
      this.log('❌ Some frontend routes are not being proxied correctly');
      Object.entries(this.testResults.frontendRouteResults).forEach(([route, result]) => {
        this.log(`   ${route}: ${result.success ? '✅' : '❌'} - ${result.description}`);
      });
    }
  }

  async scenario_TestGatewayHealth() {
    this.log('');
    this.log('🔄 SCENARIO: Verify Gateway Health and Basic Connectivity');
    this.log('Given the SystemInstaller Gateway is running');
    this.log('When I check the health endpoint');
    
    const response = await this.page.request.get('http://localhost:5000/health');
    const status = response.status();
    
    if (status === 200) {
      this.log('✅ Then the gateway responds with 200 OK');
      const data = await response.json();
      this.log(`   Gateway status: ${data.Status}`);
      this.testResults.routingResults.health = { success: true, status, data };
    } else {
      this.log('❌ Then the gateway health check fails');
      this.testResults.routingResults.health = { success: false, status };
      throw new Error(`Gateway health check failed with status ${status}`);
    }
    
    this.testResults.scenarios.push('Gateway Health Check');
  }

  async scenario_TestAuthRouteHandling() {
    this.log('');
    this.log('🔄 SCENARIO: Verify Auth Routes are NOT Proxied to Frontend');
    this.log('Given YARP is configured to exclude auth routes');
    this.log('When I test various auth endpoints');
    this.log('Then they should be handled by Gateway minimal APIs, not proxied');
    
    const authRoutes = [
      { path: '/auth/logout', method: 'GET', expectedBehavior: 'Gateway endpoint or auth challenge' },
      { path: '/auth/login', method: 'GET', expectedBehavior: 'Gateway endpoint or auth challenge' },
      { path: '/auth/user', method: 'GET', expectedBehavior: 'Gateway endpoint or auth challenge' },
      { path: '/api/protected', method: 'GET', expectedBehavior: 'Gateway endpoint or auth challenge' }
    ];

    for (const route of authRoutes) {
      this.log(`🔍 Testing ${route.method} ${route.path}`);
      
      try {
        const response = await this.page.request.get(`http://localhost:5000${route.path}`);
        const status = response.status();
        const headers = await response.allHeaders();
        const contentType = headers['content-type'] || '';
        
        // Check if this looks like a proxied response to frontend
        const isProxiedToFrontend = (
          status === 404 && 
          (contentType.includes('text/html') || contentType.includes('text/plain')) &&
          !headers['server']?.includes('Kestrel')
        );
        
        if (isProxiedToFrontend) {
          this.log(`❌ Route ${route.path} appears to be proxied to frontend (404 from web container)`);
          this.testResults.authRouteResults[route.path] = {
            success: false,
            status,
            description: 'Incorrectly proxied to frontend',
            headers
          };
        } else {
          // Status 401/403 (auth challenge) or 302 (redirect) or 200 (success) are good
          // These indicate the Gateway is handling the request
          if ([200, 302, 401, 403].includes(status)) {
            this.log(`✅ Route ${route.path} handled by Gateway (status: ${status})`);
            this.testResults.authRouteResults[route.path] = {
              success: true,
              status,
              description: 'Correctly handled by Gateway',
              headers
            };
          } else {
            this.log(`⚠️  Route ${route.path} returned unexpected status: ${status}`);
            this.testResults.authRouteResults[route.path] = {
              success: false,
              status,
              description: `Unexpected status: ${status}`,
              headers
            };
          }
        }
        
      } catch (error) {
        this.log(`❌ Error testing ${route.path}: ${error.message}`);
        this.testResults.authRouteResults[route.path] = {
          success: false,
          error: error.message,
          description: 'Request failed'
        };
      }
    }
    
    this.testResults.scenarios.push('Auth Route Handling');
  }

  async scenario_TestFrontendRouteProxying() {
    this.log('');
    this.log('🔄 SCENARIO: Verify Frontend Routes are Correctly Proxied');
    this.log('Given YARP is configured to proxy frontend routes to web container');
    this.log('When I test various frontend paths');
    this.log('Then they should be proxied to the web container');
    
    const frontendRoutes = [
      { path: '/', expectedBehavior: 'Proxied to React app' },
      { path: '/environments', expectedBehavior: 'Proxied to React app' },
      { path: '/installations', expectedBehavior: 'Proxied to React app' },
      { path: '/some-nonexistent-route', expectedBehavior: 'Proxied to React app (SPA routing)' }
    ];

    for (const route of frontendRoutes) {
      this.log(`🔍 Testing frontend route ${route.path}`);
      
      try {
        const response = await this.page.request.get(`http://localhost:5000${route.path}`);
        const status = response.status();
        const headers = await response.allHeaders();
        const contentType = headers['content-type'] || '';
        
        // For frontend routes, we expect:
        // - Status 200 (successful proxy to React)
        // - HTML content type (React app)
        // - OR redirect to auth (302) if not authenticated
        
        if (status === 200 && contentType.includes('text/html')) {
          this.log(`✅ Route ${route.path} correctly proxied to frontend`);
          this.testResults.frontendRouteResults[route.path] = {
            success: true,
            status,
            description: 'Successfully proxied to frontend',
            contentType
          };
        } else if (status === 302) {
          this.log(`✅ Route ${route.path} redirected (likely to auth) - this is correct behavior`);
          this.testResults.frontendRouteResults[route.path] = {
            success: true,
            status,
            description: 'Redirected (auth required)',
            location: headers['location']
          };
        } else {
          this.log(`❌ Route ${route.path} returned unexpected response: ${status}`);
          this.testResults.frontendRouteResults[route.path] = {
            success: false,
            status,
            description: `Unexpected response: ${status}`,
            contentType
          };
        }
        
      } catch (error) {
        this.log(`❌ Error testing ${route.path}: ${error.message}`);
        this.testResults.frontendRouteResults[route.path] = {
          success: false,
          error: error.message,
          description: 'Request failed'
        };
      }
    }
    
    this.testResults.scenarios.push('Frontend Route Proxying');
  }

  async scenario_TestSpecificLogoutIssue() {
    this.log('');
    this.log('🔄 SCENARIO: Specific Test for Logout 404 Issue');
    this.log('Given we have identified a specific 404 issue with logout');
    this.log('When I test the logout endpoint directly');
    this.log('Then it should NOT return a frontend 404 page');
    
    try {
      const response = await this.page.request.get('http://localhost:5000/auth/logout');
      const status = response.status();
      const headers = await response.allHeaders();
      const bodyText = await response.text();
      
      this.log(`Logout endpoint response: ${status}`);
      this.log(`Content-Type: ${headers['content-type']}`);
      this.log(`Server: ${headers['server'] || 'Not specified'}`);
      
      // Check if this is the problematic 404 from frontend
      const isFrontend404 = (
        status === 404 && 
        bodyText.includes('Cannot GET /auth/logout') ||
        bodyText.includes('404') && !headers['server']?.includes('Kestrel')
      );
      
      if (isFrontend404) {
        this.log('❌ PROBLEM CONFIRMED: Logout request is being proxied to frontend!');
        this.log('   This means YARP is incorrectly routing /auth/logout to the web container');
        this.testResults.routingResults.logoutIssue = {
          confirmed: true,
          status,
          description: 'Logout incorrectly proxied to frontend',
          bodyPreview: bodyText.substring(0, 200)
        };
      } else {
        this.log('✅ Logout endpoint correctly handled by Gateway');
        this.testResults.routingResults.logoutIssue = {
          confirmed: false,
          status,
          description: 'Logout correctly handled by Gateway'
        };
      }
      
    } catch (error) {
      this.log(`❌ Error testing logout endpoint: ${error.message}`);
      this.testResults.routingResults.logoutIssue = {
        error: error.message,
        description: 'Request failed'
      };
    }
    
    this.testResults.scenarios.push('Logout 404 Issue Test');
  }

  async runCompleteTest() {
    try {
      await this.setup();
      
      // Run all scenarios
      await this.scenario_TestGatewayHealth();
      await this.scenario_TestAuthRouteHandling();
      await this.scenario_TestFrontendRouteProxying();
      await this.scenario_TestSpecificLogoutIssue();
      
    } catch (error) {
      this.log(`❌ Test execution failed: ${error.message}`);
    } finally {
      await this.teardown();
    }
  }
}

// Run the test if this script is executed directly
if (require.main === module) {
  const test = new YarpRoutingTest();
  test.runCompleteTest().catch(console.error);
}

module.exports = YarpRoutingTest;
