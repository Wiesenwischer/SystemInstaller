const { chromium } = require('playwright');

async function quickBrowserTest() {
  console.log('🔍 Quick browser test starting...');
  
  try {
    // Try different launch options for Windows
    const browser = await chromium.launch({ 
      headless: false,
      channel: 'chrome', // Use installed Chrome if available
      args: [
        '--no-sandbox',
        '--disable-setuid-sandbox',
        '--disable-dev-shm-usage',
        '--disable-web-security',
        '--allow-running-insecure-content'
      ]
    });
    
    console.log('✅ Browser with Chrome channel launched!');
    const page = await browser.newPage();
    await page.goto('http://localhost:5000');
    console.log('✅ Navigated to localhost:5000 - Browser should be visible!');
    
    // Keep browser open for 10 seconds
    await page.waitForTimeout(10000);
    
    await browser.close();
    console.log('✅ Test completed!');
    
  } catch (chromeError) {
    console.log('⚠️ Chrome channel failed, trying Chromium...');
    console.log('Chrome error:', chromeError.message);
    
    try {
      const browser = await chromium.launch({ 
        headless: false,
        args: ['--no-sandbox', '--disable-setuid-sandbox']
      });
      
      console.log('✅ Chromium launched!');
      const page = await browser.newPage();
      await page.goto('http://localhost:5000');
      console.log('✅ Navigated to localhost:5000 - Browser should be visible!');
      
      await page.waitForTimeout(10000);
      await browser.close();
      console.log('✅ Test completed!');
      
    } catch (chromiumError) {
      console.error('❌ Both Chrome and Chromium failed:');
      console.error('Chromium error:', chromiumError.message);
    }
  }
}

quickBrowserTest();
