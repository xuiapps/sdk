// Every run gets a fresh worker and WASM instance; stopping it preserves the compiler.
import './xui-runtime.js';
self.addEventListener('message', async ({ data: { id, wasm } }) => {
    let lines = 0;
    let characters = 0;
    const write = (...values) => {
        if (lines > 2000 || characters > 100000) return;
        const text = values.map(value => value == null ? '' : String(value)).join(' ');
        characters += text.length;
        lines++;
        self.postMessage({ kind: 'output', id, text: text.slice(0, 100000) + '\n' });
        if (lines > 2000 || characters > 100000)
            self.postMessage({ kind: 'output', id, text: '[Output limit reached]\n' });
    };
    console.log = console.info = console.warn = console.error = console.debug = write;
    try {
        const start = performance.now();
        const module = await WebAssembly.compile(wasm, { builtins: ['js-string'] });
        const instance = await WebAssembly.instantiate(module, {
            ...globalThis.xuiRuntimeImports,
            xui: {
                'console-log': value => console.log(value),
                'console-log-boolean': value => console.log(value ? 'True' : 'False')
            }
        });
        const exitCode = instance.exports.main() ?? 0;
        self.postMessage({ kind: 'done', id, exitCode, runMs: performance.now() - start });
    } catch (error) {
        self.postMessage({ kind: 'error', id, message: error.message ?? String(error) });
    }
});
