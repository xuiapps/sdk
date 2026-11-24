import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { readFile, mkdir } from 'node:fs/promises';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { resolve } from 'node:path';

// Reuse the compiler playground's pinned browser-test dependency.
const { chromium } = createRequire(import.meta.url)('../../../Playground/browser-tests/node_modules/playwright');
const root = fileURLToPath(new URL('./Snapshots/', import.meta.url));
const screenshots = fileURLToPath(new URL('../../../Playground/browser-tests/test-output/canvas/', import.meta.url));
await mkdir(screenshots, { recursive: true });
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH });
try {
    for (const name of ['Shapes', 'Grid', 'Events', 'Values', 'FillRectRender']) {
        const snapshots = name === 'FillRectRender' ? resolve(root, '../../TestApp/Snapshots') : root;
        const page = await browser.newPage({ viewport: { width: 1000, height: 760 }, deviceScaleFactor: 1 });
        await page.context().setOffline(true);
        const network = [], errors = [];
        page.on('request', request => { if (/^https?:/.test(request.url())) network.push(request.url()); });
        page.on('pageerror', error => errors.push(String(error)));
        await page.goto(pathToFileURL(resolve(snapshots, `${name}.html`)).href);
        await page.waitForFunction(() => window.demoResult !== null && window.demoResult !== undefined);
        const result = await page.evaluate(() => window.demoResult);
        assert.equal(result.error, undefined, `${name}: ${result.error}`);
        const markdown = await readFile(resolve(snapshots, `${name}.md`), 'utf8');
        const commands = JSON.parse(markdown.match(/```json\n([\s\S]*?)\n```/)[1]);
        assert.deepEqual(result.commands, commands);
        assert.equal(result.exitCode, { Shapes: 0, Grid: 18, Events: 42, Values: 42, FillRectRender: 0 }[name]);
        const pixel = (x, y) => page.evaluate(([x, y]) => [...document.getElementById('canvas').getContext('2d').getImageData(x, y, 1, 1).data], [x, y]);
        if (name === 'FillRectRender') {
            assert.deepEqual(await pixel(10, 10), [0, 0, 0, 0]);
            for (const [x, y, expected] of [
                [40,50,[255,0,0,255]], [200,50,[0,128,0,255]],
                [300,50,[0,0,255,255]], [90,90,[255,165,0,255]], [40,190,[0,0,0,255]]
            ]) assert.deepEqual(await pixel(x,y), expected);
        } else {
            assert.deepEqual(await pixel(10, 10), [16, 32, 64, 255]);
            assert.deepEqual(await pixel(60, 120), { Shapes: [32, 128, 255, 255], Grid: [64, 191, 255, 255], Events: [128, 255, 191, 255], Values: [255, 96, 64, 255] }[name]);
            if (name === 'Values') assert.deepEqual(await pixel(50, 110), [32, 128, 255, 255]);
            if (name === 'Shapes') assert.deepEqual(await pixel(340, 120), [255, 96, 64, 255]);
            if (name === 'Grid') assert.deepEqual(await pixel(120, 120), [16, 32, 64, 255]);
            const textPixels = await page.evaluate(() => {
                const bytes = document.getElementById('canvas').getContext('2d').getImageData(40, 28, 540, 40).data;
                let painted = 0; for (let i = 0; i < bytes.length; i += 4) if (bytes[i] !== 16 || bytes[i + 1] !== 32 || bytes[i + 2] !== 64) painted++;
                return painted;
            });
            assert.ok(textPixels > 100, `${name}: label was not painted`);
        }
        await page.screenshot({ path: resolve(screenshots, `${name}.png`) });
        await page.locator('#run').click();
        await page.waitForFunction(() => window.demoResult !== null);
        assert.deepEqual(await page.evaluate(() => window.demoResult), result);
        await page.setViewportSize({ width: 390, height: 740 });
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
        assert.deepEqual(errors, []); assert.deepEqual(network, []);
        await page.close();
        console.log(`${name}: standalone file, offline execution, commands, pixels, rerun and narrow layout passed.`);
    }
} finally { await browser.close(); }
