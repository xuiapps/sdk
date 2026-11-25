import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { readFile, mkdir } from 'node:fs/promises';
import { extname, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

// Serve the published files at the actual deployment subpath, without a .NET server.
const root = resolve(process.argv[2] ?? fileURLToPath(new URL('../bin/Release/net10.0/publish/wwwroot', import.meta.url)));
const types = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css', '.wasm': 'application/wasm', '.json': 'application/json' };
const server = createServer(async (request, response) => {
    try {
        const url = new URL(request.url, 'http://localhost');
        if (!url.pathname.startsWith('/wasm/')) throw new Error('Not found');
        const path = resolve(root, decodeURIComponent(url.pathname.slice(6)) || 'index.html');
        if (!path.startsWith(root + sep)) throw new Error('Not found');
        response.setHeader('Content-Type', types[extname(path)] ?? 'application/octet-stream');
        response.end(await readFile(path));
    } catch { response.writeHead(404); response.end('Not found'); }
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH });
try {
    const page = await browser.newPage({ viewport: { width: 1440, height: 1000 } });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    const workers = [];
    page.on('worker', worker => workers.push(worker.url()));
    await page.goto(`http://127.0.0.1:${server.address().port}/wasm/`);
    await page.waitForFunction(() => !document.getElementById('run').disabled, null, { timeout: 120000 });

    async function run(source) {
        if (source !== undefined) await page.locator('#source').fill(source);
        await page.locator('#run').click();
        await page.waitForFunction(() => ['Finished', 'Failed'].includes(document.getElementById('status').textContent), null, { timeout: 30000 });
    }
    async function result(output, exit) {
        assert.equal(await page.locator('#status').innerText(), 'Finished', await page.locator('#error').textContent());
        assert.equal(await page.locator('#output').textContent(), output);
        assert.equal(await page.locator('#exit-code').textContent(), `Exit code ${exit}`);
    }

    await run();
    await result('Hello World!\n', 0);
    const download = page.waitForEvent('download');
    await page.locator('#download').click();
    const bytes = await readFile(await (await download).path());
    assert.deepEqual([...bytes.subarray(0, 4)], [0, 97, 115, 109]);

    await page.locator('#example').selectOption('loop');
    await run();
    await result('1\n3\n6\n10\n15\n', 15);
    await page.locator('#example').selectOption('generic');
    await run();
    await result('42\nTrue\n', 42);

    await run('using System; Console.WriteLine(true); Console.WriteLine(1234567890123L); Console.WriteLine(1.25f);');
    await result('True\n1234567890123\n1.25\n', 0);
    await run('using System;\nConsole.WriteLine( ;');
    assert.match(await page.locator('#error').innerText(), /Program.cs\(2,/);
    await run('throw new System.Exception("unsupported");');
    assert.equal(await page.locator('#status').innerText(), 'Failed');
    assert.notEqual(await page.locator('#error').innerText(), '');

    await run('using System; Console.WriteLine("before trap"); Box box = null; Console.WriteLine(box.Value); class Box { public int Value; }');
    assert.equal(await page.locator('#status').innerText(), 'Failed');
    assert.equal(await page.locator('#output').textContent(), 'before trap\n');

    await page.locator('#source').fill('Spin(1); static void Spin(int value) { while (value > 0) value = value | 1; }');
    await page.locator('#run').click();
    await page.waitForFunction(() => ['Running…', 'Failed'].includes(document.getElementById('status').textContent));
    assert.equal(await page.locator('#status').innerText(), 'Running…', await page.locator('#error').textContent());
    // The editor is still interactive while the other worker loops forever.
    await page.locator('#source').fill('using System; Console.WriteLine("recovered");');
    await page.locator('#stop').click();
    assert.equal(await page.locator('#status').innerText(), 'Stopped');
    await run();
    await result('recovered\n', 0);
    assert.equal(workers.filter(url => url.endsWith('/compiler-worker.js')).length, 1, 'Stopping execution must preserve the compiler');

    await run('using System; Console.WriteLine("<img src=x onerror=alert(1)>");');
    assert.equal(await page.locator('#output img').count(), 0);
    await page.locator('#example').selectOption('hello');
    await run();
    await result('Hello World!\n', 0);
    const screenshots = new URL('./test-output/', import.meta.url);
    await mkdir(screenshots, { recursive: true });
    await page.screenshot({ path: fileURLToPath(new URL('desktop.png', screenshots)), fullPage: true });
    await page.setViewportSize({ width: 390, height: 844 });
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
    await page.screenshot({ path: fileURLToPath(new URL('mobile.png', screenshots)), fullPage: true });
    const previewUrl = await page.getByRole('link', { name:'Open TestApp' }).getAttribute('href');
    assert.equal(previewUrl,'./testapp.html');
    await page.getByRole('link', { name:'Open TestApp' }).click();
    await page.waitForFunction(() => {
        const d=window.mobileDemo;
        return d?.error || (d?.ready && !d.pending && !d.api.NeedsFrame(d.session));
    });
    assert.equal(await page.evaluate(() => window.mobileDemo.error),null);
    const fps = await page.evaluate(() => { const d=window.mobileDemo; return d.api.FramesPerSecond(d.session); });
    assert.ok(fps>0 && fps<=1000,`Published TestApp FPS must update after warmup, got ${fps}`);
    const target = id => page.evaluate(id=>{
        const {api,session:s}=window.mobileDemo;
        return [api.ScreenX(s)+api.TargetX(s,id)*api.ScreenScale(s),api.ScreenY(s)+api.TargetY(s,id)*api.ScreenScale(s)];
    },id);
    const settle = () => page.waitForFunction(()=>{
        const d=window.mobileDemo;return d.error || (!d.pending && !d.api.NeedsFrame(d.session));
    });
    await page.mouse.click(...await target('CanvasDrawing'));
    await page.waitForFunction(()=>{const d=window.mobileDemo;return d.error || d.api.HasView(d.session,'Back');});
    await settle();
    assert.equal(await page.evaluate(() => window.mobileDemo.error),null);
    await page.mouse.click(...await target('Back')); await settle();
    assert.equal(await page.evaluate(()=>{const d=window.mobileDemo;return d.api.HasView(d.session,'Back');}),0);
    await page.screenshot({ path:fileURLToPath(new URL('published-testapp.png',screenshots)) });
    assert.deepEqual(errors, []);
    console.log('Browser smoke passed: examples, diagnostics, traps, console output, download, stop/re-run, worker reuse, responsive layout, published TestApp navigation and FPS.');
} finally {
    await browser.close();
    await new Promise(resolve => server.close(resolve));
}
