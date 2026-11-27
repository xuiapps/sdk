import assert from 'node:assert/strict';
import { mkdir, readFile } from 'node:fs/promises';
import { createServer } from 'node:http';
import { resolve, sep, extname } from 'node:path';
import { chromium } from 'playwright';
import { fileURLToPath } from 'node:url';

// Accept either a running host or a published directory mounted at the production path.
const input = process.argv[2] ?? 'http://127.0.0.1:5080/';
let server, url = input;
if (!/^https?:/.test(input)) {
    const root = resolve(input);
    const types = { '.html': 'text/html', '.js': 'text/javascript', '.wasm': 'application/wasm', '.json': 'application/json', '.ttf': 'font/ttf' };
    server = createServer(async (request, response) => {
        try {
            const path = new URL(request.url, 'http://localhost').pathname;
            assert.ok(path.startsWith('/wasm/'));
            const file = resolve(root, decodeURIComponent(path.slice(6)) || 'index.html');
            assert.ok(file.startsWith(root + sep));
            response.setHeader('Content-Type', types[extname(file)] ?? 'application/octet-stream');
            response.end(await readFile(file));
        } catch { response.writeHead(404); response.end('Not found'); }
    });
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    url = `http://127.0.0.1:${server.address().port}/wasm/`;
}
// CI may lack a physical GPU; allow Chromium's software WebGL driver for functional tests.
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH, args: ['--enable-unsafe-swiftshader'] });
await mkdir(new URL('./artifacts/', import.meta.url), { recursive: true });
const errors = [];
const page = await browser.newPage({ viewport: { width: 1440, height: 1100 }, deviceScaleFactor: 2 });
page.on('pageerror', error => errors.push(error.message));
page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
// Capture actual drawn text and its transformed bounds, so clicks follow layout
// rather than hardcoded screen positions. This hook is omitted from perf sampling.
await page.addInitScript(() => {
    globalThis.drawnText = [];
    const proto = CanvasRenderingContext2D.prototype;
    const reset = proto.reset, fillText = proto.fillText, drawImage = proto.drawImage;
    proto.drawImage = function (source, x, y, width, height, ...args) {
        if (this.canvas.id === 'xui-canvas' && source instanceof OffscreenCanvas && width && height) {
            const t = this.getTransform(), r = this.canvas.getBoundingClientRect();
            const center = t.transformPoint({ x: x + width / 2, y: y + height / 2 });
            globalThis.gpuBounds = { x: r.left + center.x / devicePixelRatio,
                y: r.top + center.y / devicePixelRatio, width: Math.hypot(t.a, t.b) * width / devicePixelRatio };
        }
        return drawImage.call(this, source, x, y, width, height, ...args);
    };
    proto.reset = function (...args) {
        if (this.canvas.id === 'xui-canvas') globalThis.drawnText = [];
        return reset.apply(this, args);
    };
    proto.fillText = function (text, x, y, ...args) {
        if (this.canvas.id === 'xui-canvas') {
            const m = this.measureText(text), t = this.getTransform();
            const p = t.transformPoint({ x: x + (m.actualBoundingBoxRight - m.actualBoundingBoxLeft) / 2,
                y: y + (m.actualBoundingBoxDescent - m.actualBoundingBoxAscent) / 2 });
            const r = this.canvas.getBoundingClientRect();
            globalThis.drawnText.push({ text, x: r.left + p.x / devicePixelRatio, y: r.top + p.y / devicePixelRatio });
        }
        return fillText.call(this, text, x, y, ...args);
    };
});
try {
    await page.goto(url);
    await page.waitForFunction(() => globalThis.xuiBrowserStats?.renderFps > 0 || document.querySelector('[role="alert"]'), null, { timeout: 60000 });
    assert.equal(await page.locator('[role="alert"]').count(), 0, await page.locator('body').innerText());
    assert.deepEqual(errors, []);
    if (process.env.XUI_EXPECT_AOT) assert.match(await page.locator('#runtime-mode').innerText(), /AOT/i);
    await page.screenshot({ path: fileURLToPath(new URL('./artifacts/home.png', import.meta.url)) });
    console.log('HOME TEXT', await page.evaluate(() => drawnText.map(x => x.text)));
    console.log('STARTUP', await page.evaluate(() => xuiBrowserStats));
    async function clickText(text) {
        // Navigation translates the whole page horizontally. Wait for both axes,
        // otherwise a label can be vertically stable while its hit target moves.
        await page.waitForFunction(text => {
            const p = drawnText.find(item => item.text === text);
            const previous = globalThis.clickPosition;
            const stable = p && previous?.text === text && previous.x === p.x && previous.y === p.y;
            const frames = stable ? previous.frames + 1 : 0;
            globalThis.clickPosition = p ? { ...p, frames } : null;
            return frames > 10;
        }, text);
        const target = await page.evaluate(text => drawnText.find(item => item.text === text), text);
        assert.ok(target, `Cannot find drawn text '${text}'`);
        console.log("CLICK", text, target);
        await page.mouse.move(target.x, target.y);
        await page.evaluate(async () => { for (let i = 0; i < 2; i++) await new Promise(requestAnimationFrame); });
        await page.mouse.click(target.x, target.y, { delay: 50 });
    }
    await clickText('Canvas Drawing');
    await page.waitForFunction(() => !drawnText.some(x => x.text === 'Xui SDK Examples'));
    await page.screenshot({ path: fileURLToPath(new URL('./artifacts/canvas.png', import.meta.url)) });
    assert.ok(await page.evaluate(() => drawnText.some(x => x.text === 'Fill Rect')));
    await clickText('Back');
    await page.waitForFunction(() => drawnText.some(x => x.text === 'Xui SDK Examples'));
    const menu = await page.evaluate(() => drawnText.find(x => x.text === 'Canvas Drawing'));
    await page.mouse.move(menu.x, menu.y);
    await page.mouse.wheel(0, 600);
    await page.waitForFunction(() => {
        const text = drawnText.find(x => x.text === 'Text Input');
        return text && text.y > 150 && text.y < innerHeight - 100;
    });
    await page.waitForFunction(() => {
        const p = drawnText.find(x => x.text === 'Text Input');
        const stable = globalThis.lastInputY === p?.y;
        globalThis.lastInputY = p?.y;
        globalThis.stableInputFrames = stable ? (globalThis.stableInputFrames ?? 0) + 1 : 0;
        return stableInputFrames > 10;
    });
    await clickText('Text Input');
    await page.waitForFunction(() => !drawnText.some(x => x.text === 'Xui SDK Examples'));
    const field = await page.evaluate(() => {
        const name = drawnText.find(x => x.text === 'Name:');
        return drawnText.find(x => x.text === '' && x.y > name.y);
    });
    assert.ok(field, 'Name text field is drawn');
    await page.mouse.click(field.x + 10, field.y);
    await page.keyboard.type('WASM works');
    await page.waitForFunction(() => drawnText.some(x => x.text.includes('WASM works')));
    await page.screenshot({ path: fileURLToPath(new URL('./artifacts/text-input.png', import.meta.url)) });
    await clickText('Back');
    await page.waitForFunction(() => drawnText.some(x => x.text === 'Xui SDK Examples'));
    async function scrollToText(text) {
        for (let attempt = 0; attempt < 12; attempt++) {
            const target = await page.evaluate(text => drawnText.find(x => x.text === text), text);
            if (target && target.y > 300 && target.y < 850) return;
            const bounds = await page.locator('#xui-canvas').boundingBox();
            await page.mouse.move(bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
            await page.mouse.wheel(0, target && target.y < 300 ? -300 : 300);
            await page.evaluate(async () => { for (let i = 0; i < 40; i++) await new Promise(requestAnimationFrame); });
        }
        throw new Error(`Cannot scroll '${text}' into the phone viewport`);
    }
    await scrollToText('3D');
    await clickText('3D');
    await page.waitForFunction(() => drawnText.some(x => x.text.includes('WebGL 2')) || document.querySelector('[role="alert"]'));
    assert.equal(await page.locator('[role="alert"]').count(), 0, await page.locator('body').innerText());
    await page.waitForFunction(() => xuiBrowserStats.gpu.draws > 0 && globalThis.gpuBounds);
    const drawsBeforeDrag = await page.evaluate(() => xuiBrowserStats.gpu.draws);
    const cube = await page.evaluate(() => gpuBounds);
    await page.mouse.move(cube.x, cube.y);
    await page.mouse.down();
    for (let i = 1; i <= 10; i++) {
        await page.mouse.move(cube.x + cube.width * 0.3 * i / 10, cube.y + cube.width * 0.1 * i / 10);
        await page.evaluate(() => new Promise(requestAnimationFrame));
    }
    await page.mouse.up();
    await page.waitForFunction(count => xuiBrowserStats.gpu.draws > count + 2, drawsBeforeDrag);
    console.log('GPU', await page.evaluate(() => xuiBrowserStats.gpu));
    await page.screenshot({ path: fileURLToPath(new URL('./artifacts/gpu.png', import.meta.url)) });
    assert.deepEqual(errors, []);
    await clickText('Back');
    await page.waitForFunction(() => drawnText.some(x => x.text === 'Xui SDK Examples'));
    await page.waitForFunction(() => xuiBrowserStats.gpu.resources === 1);

    // A browser may decline WebGL. The demo must remain usable and explain why.
    await page.addInitScript(() => {
        const getContext = OffscreenCanvas.prototype.getContext;
        OffscreenCanvas.prototype.getContext = function (kind, ...args) {
            return kind === 'webgl2' ? null : getContext.call(this, kind, ...args);
        };
    });
    await page.reload();
    await page.waitForFunction(() => globalThis.xuiBrowserStats?.renderFps > 0);
    await scrollToText('3D');
    await clickText('3D');
    await page.waitForFunction(() => drawnText.some(x => x.text === 'Not supported on this device'));
    assert.equal(await page.locator('[role="alert"]').count(), 0);
    await page.screenshot({ path: fileURLToPath(new URL('./artifacts/gpu-unavailable.png', import.meta.url)) });
    await clickText('Back');
    await page.waitForFunction(() => drawnText.some(x => x.text === 'Xui SDK Examples'));
    assert.deepEqual(errors, []);
    console.log('PASS: published startup, canvas, text input, WebGL rendering and disposal, unsupported-device fallback.');
} catch (error) {
    console.error(error);
    await page.screenshot({ path: fileURLToPath(new URL('./artifacts/failure.png', import.meta.url)) });
    console.log('FAILURE TEXT', await page.evaluate(() => drawnText));
    console.log('ERRORS', errors);
    throw error;
} finally {
    await browser.close();
    if (server) await new Promise(resolve => server.close(resolve));
}
