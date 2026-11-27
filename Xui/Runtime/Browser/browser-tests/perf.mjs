import { chromium } from 'playwright';
const url = new URL(process.argv[2] ?? 'http://127.0.0.1:5080/');
url.searchParams.set('benchmark', '1');
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH });
try {
    const page = await browser.newPage({ viewport: { width: 1440, height: 1100 }, deviceScaleFactor: 2 });
    const errors = [];
    page.on('pageerror', e => errors.push(e.message));
    page.on('console', m => { if (m.type() === 'error') errors.push(m.text()); });
    await page.goto(url.href);
    await page.waitForFunction(() => globalThis.xuiBrowserStats?.renderFps > 0, null, { timeout: 60000 });
    // Discard five reporting windows for Jiterpreter/font/layout warmup, then
    // collect five complete windows. There is no canvas instrumentation here.
    const samples = await page.evaluate(async () => {
        const windows = [];
        let previous = globalThis.xuiBrowserStats;
        while (windows.length < 10) {
            await new Promise(requestAnimationFrame);
            if (previous !== globalThis.xuiBrowserStats) {
                previous = globalThis.xuiBrowserStats;
                windows.push(previous);
            }
        }
        return windows.slice(5);
    });
    if (errors.length) throw new Error(errors.join('\n'));
    const mean = field => samples.reduce((total, s) => total + s[field], 0) / samples.length;
    console.log(JSON.stringify({ browser: await browser.version(), viewport: '1440x1100 @2x', mode: samples[0].mode,
        rafHz: mean('rafHz'), renderFps: mean('renderFps'), frameCpuMs: mean('frameCpuMs'), replayMs: mean('replayMs'),
        maxFrameCpuMs: Math.max(...samples.map(s => s.maxFrameCpuMs)),
        submissionsPerSecond: mean('submissions'), commandBytesPerSecond: mean('commandBytes'), measureCallsPerSecond: mean('measureCalls') }, null, 2));
} finally { await browser.close(); }
