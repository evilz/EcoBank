import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
const { chromium } = require('C:/Users/evilz/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');

const baseUrl = process.argv[2] ?? 'http://127.0.0.1:65347/';
const outputDir = dirname(fileURLToPath(import.meta.url));

const targets = [
  { name: 'browser-desktop-1440x900.png', viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1 },
  { name: 'browser-mobile-390x844.png', viewport: { width: 390, height: 844 }, deviceScaleFactor: 2, isMobile: true, hasTouch: true }
];

const browser = await chromium.launch({ headless: true });

for (const target of targets) {
  const context = await browser.newContext({
    viewport: target.viewport,
    deviceScaleFactor: target.deviceScaleFactor,
    isMobile: target.isMobile ?? false,
    hasTouch: target.hasTouch ?? false
  });
  const page = await context.newPage();
  const messages = [];
  page.on('console', message => messages.push(`${message.type()}: ${message.text()}`));
  page.on('pageerror', error => messages.push(`pageerror: ${error.message}`));
  await page.goto(baseUrl, { waitUntil: 'networkidle', timeout: 60000 });
  try {
    await page.locator('canvas').first().waitFor({ state: 'visible', timeout: 60000 });
  } catch {
    await page.screenshot({ path: join(outputDir, target.name.replace('.png', '-failed.png')), fullPage: true });
    console.log(messages.join('\n'));
    throw new Error(`Avalonia canvas was not visible for ${target.name}`);
  }
  await page.waitForTimeout(3000);
  await page.screenshot({ path: join(outputDir, target.name), fullPage: true });
  await context.close();
}

await browser.close();
