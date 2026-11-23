import { dotnet } from './_framework/dotnet.js';

try {
    const runtime = await dotnet.withDiagnosticTracing(false).create();
    const exports = await runtime.getAssemblyExports(runtime.getConfig().mainAssemblyName);
    const compile = exports.Xui.Compiler.Playground.Program.Compile;
    self.addEventListener('message', ({ data: { id, source } }) => {
        try {
            const start = performance.now();
            const wasm = compile(source);
            self.postMessage({ kind: 'compiled', id, wasm, compileMs: performance.now() - start }, [wasm.buffer]);
        } catch (error) {
            self.postMessage({ kind: 'error', id, message: error.message ?? String(error) });
        }
    });
    self.postMessage({ kind: 'ready' });
} catch (error) {
    self.postMessage({ kind: 'startup-error', message: error.message ?? String(error) });
}
