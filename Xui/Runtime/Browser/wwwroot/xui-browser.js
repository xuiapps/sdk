import { createPlayer } from './canvas-player.js';
import { createGpuHost } from './webgl.js';

const gpu = createGpuHost();
export function gpuInvoke(...args) { return gpu.invoke(...args); }
export function gpuUpload(...args) { gpu.upload(...args); }

let canvas, context, measureContext, exports, editor;
const images = new Map();
let player, platform;
let dispatchPending = false, failed = false;
export function scheduleDispatch() {
    if (dispatchPending) return;
    dispatchPending = true;
    queueMicrotask(() => {
        dispatchPending = false;
        try { platform.Dispatch(); } catch (error) { showError(error); }
    });
}
let replayMs = 0, submissions = 0, commandBytes = 0, measureCalls = 0;
const metricBuffer = new Float64Array(12);

export function setTitle(title) {
    document.title = title;
    document.getElementById('app-title')?.replaceChildren(title);
}
export function setKeyboard(required) {
    if (required) editor.focus({ preventScroll: true });
    else if (document.activeElement === editor) canvas.focus({ preventScroll: true });
}
export function beginFrame(ratio) {
    context.reset();
    context.setTransform(ratio, 0, 0, ratio, 0, 0);
    context.font = '15px Inter';
}
export function submit(commands, strings) {
    const start = performance.now();
    player(commands, strings);
    replayMs += performance.now() - start;
    submissions++;
    commandBytes += commands.byteLength;
}
export function measureText(font, text, result) {
    measureContext.font = font;
    measureContext.textAlign = 'left';
    measureContext.textBaseline = 'alphabetic';
    const m = measureContext.measureText(text);
    metricBuffer.set([m.width, m.actualBoundingBoxLeft, m.actualBoundingBoxRight,
        m.actualBoundingBoxAscent, m.actualBoundingBoxDescent,
        m.fontBoundingBoxAscent ?? m.actualBoundingBoxAscent, m.fontBoundingBoxDescent ?? m.actualBoundingBoxDescent,
        m.emHeightAscent ?? m.fontBoundingBoxAscent ?? m.actualBoundingBoxAscent,
        m.emHeightDescent ?? m.fontBoundingBoxDescent ?? m.actualBoundingBoxDescent,
        m.alphabeticBaseline ?? 0, m.hangingBaseline ?? 0, m.ideographicBaseline ?? 0]);
    result.set(metricBuffer);
    measureCalls++;
}
export async function loadImage(id, uri) {
    const entry = {};
    images.set(id, entry);
    const image = new Image();
    const path = uri.startsWith('asset://') ? `Assets/${uri.slice(8)}` : uri;
    image.src = new URL(path, document.baseURI).href;
    await image.decode();
    if (images.get(id) === entry) entry.source = image;
    return { width: image.naturalWidth, height: image.naturalHeight };
}
export function releaseImage(id) { images.delete(id); }
export function uploadImage(id, width, height, pixels) {
    const bytes = pixels.slice();
    for (let i = 0; i < bytes.length; i += 4) {
        const blue = bytes[i]; bytes[i] = bytes[i + 2]; bytes[i + 2] = blue;
    }
    const source = new OffscreenCanvas(width, height);
    source.getContext('2d').putImageData(new ImageData(new Uint8ClampedArray(bytes.buffer), width, height), 0, 0);
    images.set(id, { source });
}

export async function initialize(mode) {
    canvas = document.getElementById('xui-canvas');
    if (!canvas) throw new Error('The Xui browser host needs a canvas with id="xui-canvas".');
    context = canvas.getContext('2d');
    measureContext = document.createElement('canvas').getContext('2d');
    player = createPlayer(context, images, gpu);
    const fontStyles = [['Regular', 400, 'normal'], ['Medium', 500, 'normal'], ['SemiBold', 600, 'normal'],
        ['Bold', 700, 'normal'], ['Italic', 400, 'italic'], ['BoldItalic', 700, 'italic']];
    await Promise.all(fontStyles.map(async ([file, weight, style]) => {
        const face = new FontFace('Inter', `url(${new URL(`fonts/Inter-${file}.ttf`, document.baseURI)})`, { weight: String(weight), style });
        document.fonts.add(await face.load());
    }));
    const assembly = await globalThis.getDotnetRuntime(0).getAssemblyExports('Xui.Runtime.Browser');
    exports = assembly.Xui.Runtime.Browser.Actual.BrowserWindow;
    platform = assembly.Xui.Runtime.Browser.Actual.BrowserPlatform;
    document.getElementById('runtime-mode')?.replaceChildren(mode);

    editor = document.createElement('textarea');
    editor.setAttribute('aria-label', 'Xui text input');
    editor.autocapitalize = 'off'; editor.autocomplete = 'off'; editor.spellcheck = false;
    Object.assign(editor.style, { position: 'fixed', left: '0', bottom: '0', width: '1px', height: '1px', opacity: '0', pointerEvents: 'none' });
    document.body.append(editor);
    const keyCodes = { Backspace: 8, Tab: 9, Enter: 13, Escape: 27, PageUp: 33, PageDown: 34,
        End: 35, Home: 36, ArrowLeft: 37, ArrowUp: 38, ArrowRight: 39, ArrowDown: 40, Insert: 45, Delete: 46 };
    for (const target of [canvas, editor]) target.addEventListener('keydown', event => {
        if (event.isComposing || event.ctrlKey || event.metaKey || event.altKey) return;
        const key = keyCodes[event.key] ?? (event.key.length === 1 ? event.key.toUpperCase().charCodeAt(0) : 0);
        const handled = exports.OnKey(key, event.shiftKey, event.repeat);
        if (handled) event.preventDefault();
        else if (target === canvas && event.key.length === 1) { exports.OnText(event.key); event.preventDefault(); }
    });
    let composing = false;
    editor.addEventListener('compositionstart', () => { composing = true; });
    editor.addEventListener('compositionend', () => {
        composing = false;
        if (editor.value) exports.OnText(editor.value);
        editor.value = '';
    });
    editor.addEventListener('input', () => {
        if (composing) return;
        if (editor.value) exports.OnText(editor.value);
        editor.value = '';
    });
    const point = event => {
        const rect = canvas.getBoundingClientRect();
        return [event.clientX - rect.left, event.clientY - rect.top];
    };
    for (const [name, phase] of [['pointerdown', 0], ['pointermove', 1], ['pointerup', 2], ['pointercancel', 3]]) {
        canvas.addEventListener(name, event => {
            if (phase === 0) {
                if (document.activeElement !== editor) canvas.focus({ preventScroll: true });
                canvas.setPointerCapture(event.pointerId);
            }
            exports.OnPointer(phase, event.pointerId, event.pointerType !== 'mouse', event.button, ...point(event));
            if (phase >= 2 && canvas.hasPointerCapture(event.pointerId)) canvas.releasePointerCapture(event.pointerId);
            event.preventDefault();
        });
    }
    canvas.addEventListener('contextmenu', event => event.preventDefault());
    canvas.addEventListener('wheel', event => {
        const factor = event.deltaMode === 1 ? 16 : event.deltaMode === 2 ? canvas.clientHeight : 1;
        exports.OnWheel(...point(event), -event.deltaX * factor, -event.deltaY * factor);
        event.preventDefault();
    }, { passive: false });
    const continuous = document.getElementById('continuous');
    if (continuous) continuous.checked = new URLSearchParams(location.search).has('benchmark');
    const status = document.getElementById('status');
    let started = performance.now(), ticks = 0, frames = 0, totalMs = 0, maxMs = 0;
    function frame(timestamp) {
        if (failed) return;
        try {
            const width = canvas.clientWidth, height = canvas.clientHeight, ratio = devicePixelRatio || 1;
            const pixelsWide = Math.round(width * ratio), pixelsHigh = Math.round(height * ratio);
            if (canvas.width !== pixelsWide || canvas.height !== pixelsHigh) {
                canvas.width = pixelsWide; canvas.height = pixelsHigh;
            }
            if (continuous?.checked) exports.InvalidateFrame();
            const start = performance.now();
            const rendered = exports.OnAnimationFrame(width, height, timestamp, ratio);
            const elapsed = performance.now() - start;
            ticks++;
            if (rendered) { frames++; totalMs += elapsed; maxMs = Math.max(maxMs, elapsed); }
            const interval = timestamp - started;
            if (interval >= 1000) {
                // Public diagnostics for repeatable perf runs; these exclude asynchronous browser raster/compositing work.
                const stats = { mode, rafHz: ticks * 1000 / interval, renderFps: frames * 1000 / interval,
                    frameCpuMs: frames ? totalMs / frames : 0, maxFrameCpuMs: maxMs,
                    replayMs: frames ? replayMs / frames : 0, submissions, commandBytes, measureCalls, gpu: gpu.stats };
                globalThis.xuiBrowserStats = stats;
                if (status) status.textContent = `${stats.rafHz.toFixed(0)} Hz · ${stats.renderFps.toFixed(0)} fps · ${stats.frameCpuMs.toFixed(2)} ms CPU · ${stats.replayMs.toFixed(2)} ms canvas`;
                started = timestamp; ticks = frames = totalMs = maxMs = replayMs = submissions = commandBytes = measureCalls = 0;
            }
            if (rendered) document.getElementById('loading')?.remove();
            requestAnimationFrame(frame);
        } catch (error) { showError(error); }
    }
    requestAnimationFrame(frame);
}
export function showError(error) {
    failed = true;
    console.error(error);
    const loading = document.getElementById('loading') ?? document.body.appendChild(document.createElement('pre'));
    loading.id = 'loading'; loading.textContent = `Xui failed to start or render:\n${error.stack ?? error}`;
    loading.setAttribute('role', 'alert');
}
