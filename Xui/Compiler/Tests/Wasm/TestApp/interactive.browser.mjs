import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
const { chromium } = createRequire(import.meta.url)('../../../Playground/browser-tests/node_modules/playwright');
const output = new URL('../../../Playground/browser-tests/test-output/interactive/', import.meta.url);
await mkdir(output, { recursive: true });
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH });
try {
    const page = await browser.newPage({ viewport: { width: 1000, height: 1000 }, deviceScaleFactor: 2, hasTouch: true });
    await page.context().setOffline(true);
    const errors = [], network = [];
    page.on('pageerror', e => errors.push(String(e)));
    page.on('request', r => { if (/^https?:/.test(r.url())) network.push(r.url()); });
    await page.goto(new URL(process.argv[2] ?? './Snapshots/BrowserInteractive.html', import.meta.url).href);
    await page.waitForFunction(() => window.mobileDemo?.ready || window.mobileDemo?.error);
    const healthy = async () => assert.equal(await page.evaluate(() => window.mobileDemo.error), null);
    const settle = async () => {
        await page.waitForFunction(() => {
            const d=window.mobileDemo; return d.error || (!d.pending && !d.api.NeedsFrame(d.session));
        });
        await healthy();
    };
    const target = id => page.evaluate(id => {
        const {api,session:s}=window.mobileDemo;
        return [api.ScreenX(s)+api.TargetX(s,id)*api.ScreenScale(s), api.ScreenY(s)+api.TargetY(s,id)*api.ScreenScale(s)];
    },id);
    const has = id => page.evaluate(id => { const d=window.mobileDemo; return d.api.HasView(d.session,id); },id);
    const pixel = (x,y) => page.evaluate(([x,y]) => [...document.getElementById('canvas').getContext('2d')
        .getImageData(Math.round(x*devicePixelRatio),Math.round(y*devicePixelRatio),1,1).data],[x,y]);
    const screen = (x,y) => page.evaluate(([x,y]) => {
        const {api,session:s}=window.mobileDemo;
        return [api.ScreenX(s)+x*api.ScreenScale(s),api.ScreenY(s)+y*api.ScreenScale(s)];
    },[x,y]);
    await settle();
    assert.equal(await has('CanvasDrawing'),1);
    assert.equal(await page.evaluate(() => document.fonts.check('600 20px Inter')),true);
    assert.deepEqual(await pixel(10,10),[255,255,255,255]);
    assert.deepEqual(await pixel(...await screen(200,29)),[17,17,17,255]);
    await page.screenshot({ path:fileURLToPath(new URL('home.png',output)) });

    // Cancelled presses must not navigate or leave capture/animations stuck.
    await page.mouse.move(...await target('CanvasDrawing')); await page.mouse.down();
    await page.evaluate(() => window.dispatchEvent(new Event('blur')));
    await page.mouse.up(); await settle();
    assert.equal(await has('Back'),0);
    await page.mouse.click(...await target('CanvasDrawing'));
    await page.waitForFunction(() => { const d=window.mobileDemo; return d.error || d.api.HasView(d.session,'Back'); });
    await settle();
    assert.equal(await has('Back'),1);
    assert.deepEqual(await pixel(...await screen(20,130)),[255,0,0,255]);
    assert.deepEqual(await pixel(...await screen(150,130)),[0,128,0,255]);
    assert.deepEqual(await pixel(...await screen(280,130)),[0,0,255,255]);
    await page.screenshot({ path:fileURLToPath(new URL('canvas.png',output)) });
    await page.mouse.click(...await target('Back')); await settle();
    assert.equal(await has('Back'),0);
    assert.equal(await has('CanvasDrawing'),1);
    const frames = await page.evaluate(() => { const d=window.mobileDemo; return d.api.Frames(d.session); });
    await page.evaluate(() => new Promise(resolve=>requestAnimationFrame(()=>requestAnimationFrame(resolve))));
    assert.equal(await page.evaluate(() => { const d=window.mobileDemo; return d.api.Frames(d.session); }),frames);

    await page.setViewportSize({ width:390, height:740 });
    await page.waitForFunction(() => document.getElementById('canvas').width === 780);
    await settle();
    await page.touchscreen.tap(...await target('CanvasDrawing'));
    await page.waitForFunction(() => { const d=window.mobileDemo; return d.error || d.api.HasView(d.session,'Back'); });
    await settle();
    assert.deepEqual(await pixel(...await screen(20,130)),[255,0,0,255]);
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth<=innerWidth),true);
    await page.screenshot({ path:fileURLToPath(new URL('narrow.png',output)) });
    await page.touchscreen.tap(...await target('Back')); await settle();
    assert.equal(await has('Back'),0);
    await page.evaluate(() => window.mobileDemo.dispose());
    assert.equal(await page.evaluate(() => window.mobileDemo.disposed && !window.mobileDemo.pending && window.mobileDemo.session===null),true);
    assert.deepEqual(errors,[]); assert.deepEqual(network,[]);
    console.log('Interactive TestApp: offline home, middleware phone, cancellation, mouse/touch navigation and Back, RGB pixels, DPR resize, idle and disposal passed.');
} finally { await browser.close(); }
