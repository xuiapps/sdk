import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';
const { chromium } = createRequire(import.meta.url)('../../../Playground/browser-tests/node_modules/playwright');
const screenshots = fileURLToPath(new URL('../../../Playground/browser-tests/test-output/mobile/', import.meta.url));
await mkdir(screenshots, { recursive: true });
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH });
try {
    const page = await browser.newPage({ viewport: { width: 1100, height: 1000 }, deviceScaleFactor: 2, hasTouch: true });
    await page.context().setOffline(true);
    const network = [], errors = [];
    page.on('request', request => { if (/^https?:/.test(request.url())) network.push(request.url()); });
    page.on('pageerror', error => errors.push(String(error)));
    await page.goto(new URL('./Snapshots/MobilePhone.html', import.meta.url).href);
    await page.waitForFunction(() => window.mobileDemo?.ready || window.mobileDemo?.error);
    assert.equal(await page.evaluate(() => window.mobileDemo.error), null);
    const state = () => page.evaluate(() => {
        const { api, session, pending } = window.mobileDemo;
        return { x: api.ScreenX(session), y: api.ScreenY(session), scale: api.ScreenScale(session),
            frames: api.Frames(session), events: api.PointerEvents(session), animations: api.Animations(session),
            markerX: api.MarkerX(session), markerY: api.MarkerY(session), needsFrame: api.NeedsFrame(session), pending };
    });
    const settle = () => page.waitForFunction(() => {
        const d = window.mobileDemo; return d.error || (!d.pending && !d.api.NeedsFrame(d.session));
    });
    const pixel = (x, y) => page.evaluate(([x,y]) => [...document.getElementById('canvas').getContext('2d')
        .getImageData(Math.round(x*devicePixelRatio), Math.round(y*devicePixelRatio),1,1).data], [x,y]);
    let phone = await state();
    const at = (x,y) => [phone.x + x*phone.scale, phone.y + y*phone.scale];
    assert.deepEqual(await pixel(10,10), [255,255,255,255]);
    assert.deepEqual(await pixel(...at(20,80)), [255,0,0,255]);
    assert.deepEqual(await pixel(...at(150,80)), [0,128,0,255]);
    assert.deepEqual(await pixel(...at(280,80)), [0,0,255,255]);
    assert.deepEqual(await pixel(...at(200,29)), [17,17,17,255]); // actual middleware island
    assert.deepEqual(await pixel(...at(-10,400)), [184,184,190,255]); // aluminum frame
    await page.screenshot({ path: resolve(screenshots, 'phone.png') });

    await page.mouse.move(...at(200,400)); await page.mouse.down();
    await page.mouse.move(...at(220,420), { steps: 3 });
    await page.waitForFunction(() => { const d = window.mobileDemo; return !d.pending; });
    let moved = await state();
    assert.ok(Math.abs(moved.markerX - 220) < .01); assert.ok(Math.abs(moved.markerY - 420) < .01);
    assert.deepEqual(await pixel(...at(220,420)), [102,51,221,255]);
    await page.screenshot({ path: resolve(screenshots, 'contact.png') });
    await page.mouse.up(); await settle();
    assert.deepEqual(await pixel(...at(220,420)), [255,255,255,255]);
    assert.ok((await state()).animations > 1);
    assert.equal(await page.evaluate(() => { const c = document.getElementById('canvas'); return c.hasPointerCapture(1); }), false);

    const idle = (await state()).frames;
    await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));
    assert.equal((await state()).frames, idle, 'idle view must not redraw continuously');
    await page.mouse.move(...at(180,380)); await page.mouse.down();
    await page.evaluate(() => window.dispatchEvent(new Event('blur')));
    await page.mouse.up(); await settle();
    assert.equal(await page.evaluate(() => document.getElementById('canvas').hasPointerCapture(1)), false);

    await page.setViewportSize({ width:390, height:740 });
    await page.waitForFunction(() => document.getElementById('canvas').width === 780 && !window.mobileDemo.pending);
    phone = await state();
    assert.ok(phone.scale < 1);
    const eventsBeforeTouch = phone.events;
    await page.touchscreen.tap(...at(180,400)); await settle();
    assert.ok((await state()).events >= eventsBeforeTouch + 2);
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
    assert.deepEqual(await pixel(...at(20,80)), [255,0,0,255]);
    await page.screenshot({ path: resolve(screenshots, 'narrow.png') });
    const stopped = await page.evaluate(async () => {
        const d = window.mobileDemo, { api, session } = d;
        const before = api.Frames(session); d.dispose(); d.invalidate();
        await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
        return { before, after: api.Frames(session), pending: d.pending, released: d.session === null && d.api === null };
    });
    assert.equal(stopped.before, stopped.after); assert.equal(stopped.pending, false); assert.equal(stopped.released, true);
    assert.deepEqual(errors, []); assert.deepEqual(network, []);
    console.log('Mobile: offline phone pixels, DPR, pointer drag/cancel, touch, animation/idle, resize and disposal passed.');
} finally { await browser.close(); }
