async function startMobile(bytes) {
    const canvas = document.getElementById('canvas');
    const status = document.getElementById('status');
    const context = canvas.getContext('2d');
    const commands = [];
    const module = await WebAssembly.compile(bytes, { builtins: ['js-string'] });
    const { exports: api } = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas: canvasImports(context, commands) });
    let session = api.Create();
    let pending = 0, disposed = false;
    const contacts = new Map();
    const demo = { api, session, commands, ready: false, error: null, pending: false, disposed: false, invalidate, dispose };
    window.mobileDemo = demo;

    function fail(error) {
        demo.error = String(error); status.textContent = `Unable to render: ${error.message ?? error}`;
        dispose();
    }
    function invalidate() {
        if (disposed || pending || document.hidden) return;
        demo.pending = true;
        pending = requestAnimationFrame(frame);
    }
    function frame(time) {
        pending = 0; demo.pending = false;
        if (disposed) return;
        try {
            const { width, height } = canvas.getBoundingClientRect();
            const ratio = devicePixelRatio || 1;
            const pixelWidth = Math.max(1, Math.round(width * ratio)), pixelHeight = Math.max(1, Math.round(height * ratio));
            if (canvas.width !== pixelWidth || canvas.height !== pixelHeight) {
                canvas.width = pixelWidth; canvas.height = pixelHeight;
            }
            context.setTransform(ratio, 0, 0, ratio, 0, 0);
            context.globalAlpha = 1; context.fillStyle = 'white'; context.fillRect(0, 0, width, height);
            commands.length = 0;
            api.Frame(session, width, height, BigInt(Math.round(time * 10000)));
            demo.ready = true;
            if (api.NeedsFrame(session)) invalidate();
        } catch (error) { fail(error); }
    }
    function send(kind, event) {
        const bounds = canvas.getBoundingClientRect();
        return api.Pointer(session, kind, event.pointerId, event.clientX - bounds.left, event.clientY - bounds.top, event.pressure || 0);
    }
    function pointer(event) {
        if (disposed || !demo.ready) return;
        try {
            const kind = { pointerdown: 2, pointermove: 3, pointerup: 4, pointercancel: 5, lostpointercapture: 5 }[event.type];
            if (kind !== 2 && !contacts.has(event.pointerId)) return;
            const sample = event.type === 'lostpointercapture' ? contacts.get(event.pointerId) : event;
            if (!send(kind, sample)) return;
            event.preventDefault();
            if (kind === 2) {
                contacts.set(event.pointerId, sample); canvas.setPointerCapture(event.pointerId); canvas.focus({ preventScroll: true });
            } else if (kind === 3) contacts.set(event.pointerId, sample);
            else {
                contacts.delete(event.pointerId);
                if (canvas.hasPointerCapture(event.pointerId)) canvas.releasePointerCapture(event.pointerId);
            }
            invalidate();
        } catch (error) { fail(error); }
    }
    function cancelContacts() {
        for (const [id, event] of contacts) {
            send(5, event); contacts.delete(id);
            if (canvas.hasPointerCapture(id)) canvas.releasePointerCapture(id);
        }
        invalidate();
    }
    function visibility() {
        if (document.hidden) {
            cancelContacts(); cancelAnimationFrame(pending); pending = 0; demo.pending = false;
        } else invalidate();
    }
    const events = ['pointerdown', 'pointermove', 'pointerup', 'pointercancel', 'lostpointercapture'];
    const observer = new ResizeObserver(invalidate);
    function dispose() {
        if (disposed) return;
        disposed = true;
        cancelAnimationFrame(pending); pending = 0; demo.pending = false;
        observer.disconnect();
        for (const name of events) canvas.removeEventListener(name, pointer);
        document.removeEventListener('visibilitychange', visibility);
        window.removeEventListener('blur', cancelContacts);
        window.removeEventListener('pagehide', dispose);
        for (const id of contacts.keys()) if (canvas.hasPointerCapture(id)) canvas.releasePointerCapture(id);
        contacts.clear();
        session = null; demo.session = null; demo.api = null; demo.disposed = true;
        commands.length = 0;
    }
    for (const name of events) canvas.addEventListener(name, pointer, { passive: false });
    observer.observe(canvas);
    document.addEventListener('visibilitychange', visibility);
    window.addEventListener('blur', cancelContacts);
    window.addEventListener('pagehide', dispose);
    invalidate();
}
