const { chromium } = require('playwright');

async function debugLogout() {
  const browser = await chromium.launch({ headless: false, slowMo: 1000 });
  const page = await browser.newPage();
  
  try {
    console.log('1. Going directly to logout endpoint...');
    await page.goto('http://localhost:5000/auth/logout');
    await page.waitForTimeout(3000);
    
    console.log('2. Current URL:', page.url());
    console.log('3. Page title:', await page.title());
    
    const bodyText = await page.locator('body').textContent();
    console.log('4. Page content preview:', bodyText.substring(0, 200));
    
    if (bodyText.includes('404') || bodyText.includes('Not Found')) {
      console.log('💥 404 ERROR DETECTED!');
      console.log('Full page text:', bodyText);
    }
    
    await page.waitForTimeout(5000);
    
  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await browser.close();
  }
}

debugLogout();
