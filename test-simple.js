const { chromium } = require('playwright');

async function simpleTest() {
  console.log('🚀 Starting simple login test...');
  
  const browser = await chromium.launch({ 
    headless: false,
    slowMo: 1000
  });
  
  const page = await browser.newPage();
  
  try {
    console.log('📍 Going to gateway...');
    await page.goto('http://localhost:5000');
    
    console.log('⏳ Waiting for redirect to Keycloak...');
    await page.waitForTimeout(5000);
    
    const currentUrl = page.url();
    console.log(`📍 Current URL: ${currentUrl}`);
    
    if (currentUrl.includes('keycloak')) {
      console.log('✅ Redirected to Keycloak!');
      
      // Take screenshot
      await page.screenshot({ path: 'simple-keycloak.png' });
      console.log('📸 Screenshot saved');
      
      // Look for all form elements
      const inputs = await page.locator('input').all();
      console.log(`Found ${inputs.length} input elements`);
      
      for (let i = 0; i < inputs.length; i++) {
        const type = await inputs[i].getAttribute('type');
        const name = await inputs[i].getAttribute('name');
        const id = await inputs[i].getAttribute('id');
        console.log(`Input ${i}: type=${type}, name=${name}, id=${id}`);
      }
      
      const buttons = await page.locator('button, input[type="submit"]').all();
      console.log(`Found ${buttons.length} buttons`);
      
      for (let i = 0; i < buttons.length; i++) {
        const text = await buttons[i].textContent();
        const value = await buttons[i].getAttribute('value');
        const type = await buttons[i].getAttribute('type');
        console.log(`Button ${i}: text="${text}", value="${value}", type=${type}`);
      }
    } else {
      console.log(`❌ Not redirected to Keycloak. Current URL: ${currentUrl}`);
    }
    
    console.log('⏳ Keeping browser open for inspection...');
    await page.waitForTimeout(10000);
    
  } catch (error) {
    console.error('❌ Error:', error.message);
  } finally {
    await browser.close();
    console.log('✅ Test completed');
  }
}

simpleTest();
