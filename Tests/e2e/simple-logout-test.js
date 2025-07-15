#!/usr/bin/env node

/**
 * SystemInstaller Logout E2E Test - PURE NODE.JS VERSION
 * No external dependencies - uses built-in modules only
 * Just run: node simple-logout-test.js
 */

const https = require('https');
const http = require('http');

class SimpleLogoutTest {
  constructor() {
    this.baseUrl = 'http://localhost:5000';
    this.steps = [];
  }

  log(message) {
    console.log(message);
    this.steps.push(`${new Date().toISOString()}: ${message}`);
  }

  async httpRequest(url, options = {}) {
    return new Promise((resolve, reject) => {
      const protocol = url.startsWith('https') ? https : http;
      const req = protocol.request(url, {
        method: options.method || 'GET',
        headers: options.headers || {},
        ...options
      }, (res) => {
        let data = '';
        res.on('data', chunk => data += chunk);
        res.on('end', () => {
          resolve({
            status: res.statusCode,
            headers: res.headers,
            data: data,
            url: res.url || url
          });
        });
      });
      
      req.on('error', reject);
      
      if (options.body) {
        req.write(options.body);
      }
      
      req.end();
    });
  }

  async testHealthCheck() {
    this.log('');
    this.log('🔄 SCENARIO: Health Check');
    this.log('Given SystemInstaller should be running');
    this.log('When I check the health endpoint');
    
    try {
      const response = await this.httpRequest(`${this.baseUrl}/health`);
      this.log(`Then the health status should be: ${response.status}`);
      
      if (response.status === 200) {
        this.log('✅ SystemInstaller is running');
        const healthData = JSON.parse(response.data);
        this.log(`Health response: ${JSON.stringify(healthData)}`);
        return true;
      } else {
        this.log('❌ SystemInstaller health check failed');
        return false;
      }
    } catch (error) {
      this.log(`❌ Cannot connect to SystemInstaller: ${error.message}`);
      this.log('Make sure docker-compose up -d is running');
      return false;
    }
  }

  async testAuthEndpoint() {
    this.log('');
    this.log('🔄 SCENARIO: Authentication Endpoint Test');
    this.log('Given I want to check authentication status');
    this.log('When I call the auth/user endpoint');
    
    try {
      const response = await this.httpRequest(`${this.baseUrl}/auth/user`);
      this.log(`Then the auth status should be: ${response.status}`);
      
      if (response.status === 200) {
        this.log('✅ User is currently authenticated');
        try {
          const userInfo = JSON.parse(response.data);
          this.log(`Authenticated as: ${userInfo.username || userInfo.sub || 'Unknown'}`);
        } catch (e) {
          this.log('User info parse failed');
        }
        return 'authenticated';
      } else if (response.status === 401) {
        this.log('ℹ️  User is not authenticated (expected)');
        return 'not-authenticated';
      } else {
        this.log(`⚠️  Unexpected auth status: ${response.status}`);
        return 'unknown';
      }
    } catch (error) {
      this.log(`❌ Auth endpoint test failed: ${error.message}`);
      return 'error';
    }
  }

  async testLogoutEndpoint() {
    this.log('');
    this.log('🔄 SCENARIO: Logout Endpoint Test');
    this.log('Given I want to test the logout functionality');
    this.log('When I call the logout endpoint directly');
    
    try {
      const response = await this.httpRequest(`${this.baseUrl}/auth/logout`);
      this.log(`Then the logout response status should be: ${response.status}`);
      
      if (response.status === 302 || response.status === 301) {
        const location = response.headers.location;
        this.log(`✅ Logout redirects to: ${location}`);
        
        if (location && location.includes('localhost:8082')) {
          this.log('✅ Correctly redirects to Keycloak logout');
          return 'redirect-to-keycloak';
        } else {
          this.log('⚠️  Redirects but not to Keycloak');
          return 'redirect-other';
        }
      } else {
        this.log(`⚠️  Unexpected logout response: ${response.status}`);
        return 'unexpected';
      }
    } catch (error) {
      this.log(`❌ Logout endpoint test failed: ${error.message}`);
      return 'error';
    }
  }

  async testProtectedEndpoints() {
    this.log('');
    this.log('🔄 SCENARIO: Protected Endpoints After Logout');
    this.log('Given the user should be logged out');
    this.log('When I test protected endpoints');
    
    const endpoints = ['/auth/user', '/api/protected'];
    let allProtected = true;
    
    for (const endpoint of endpoints) {
      try {
        const response = await this.httpRequest(`${this.baseUrl}${endpoint}`);
        this.log(`Then ${endpoint} should return: ${response.status} (expected: 401)`);
        
        if (response.status !== 401) {
          this.log(`❌ ${endpoint} is still accessible! Status: ${response.status}`);
          allProtected = false;
        } else {
          this.log(`✅ ${endpoint} properly protected`);
        }
      } catch (error) {
        this.log(`❌ Error testing ${endpoint}: ${error.message}`);
        allProtected = false;
      }
    }
    
    return allProtected;
  }

  async runDiagnosticTest() {
    this.log('🚀 SystemInstaller Logout Diagnostic Test');
    this.log('==========================================');
    this.log('This test will check the current logout implementation');
    this.log('');
    
    // Test 1: Health Check
    const healthOk = await this.testHealthCheck();
    if (!healthOk) {
      this.log('❌ Cannot proceed - SystemInstaller not running');
      return;
    }
    
    // Test 2: Initial Auth Status
    const initialAuth = await this.testAuthEndpoint();
    
    // Test 3: Logout Endpoint
    const logoutResult = await this.testLogoutEndpoint();
    
    // Test 4: Auth Status After Logout
    await this.testAuthEndpoint();
    
    // Test 5: Protected Endpoints
    const protectedOk = await this.testProtectedEndpoints();
    
    // Summary
    this.log('');
    this.log('📊 DIAGNOSTIC SUMMARY');
    this.log('=====================');
    this.log(`SystemInstaller Health: ${healthOk ? '✅ OK' : '❌ FAIL'}`);
    this.log(`Initial Auth Status: ${initialAuth}`);
    this.log(`Logout Endpoint: ${logoutResult}`);
    this.log(`Protected Endpoints: ${protectedOk ? '✅ OK' : '❌ FAIL'}`);
    
    this.log('');
    this.log('🎯 MANUAL TEST INSTRUCTIONS:');
    this.log('=============================');
    this.log('1. Open browser to: http://localhost:5000');
    this.log('2. Login with admin/admin');
    this.log('3. Click logout button (or go to /auth/logout)');
    this.log('4. 🔴 CRITICAL: Hit F5 (refresh)');
    this.log('5. Check what you see:');
    this.log('   ✅ CORRECT: Login page (must enter credentials)');
    this.log('   ❌ BUG: Dashboard (automatically logged in)');
    this.log('');
    this.log('If you see the dashboard after F5, the Keycloak session');
    this.log('was not properly terminated during logout.');
  }
}

// Run the diagnostic test
if (require.main === module) {
  const test = new SimpleLogoutTest();
  test.runDiagnosticTest().catch(console.error);
}

module.exports = SimpleLogoutTest;
