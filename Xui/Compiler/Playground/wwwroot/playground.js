const $ = id => document.getElementById(id);
const source = $('source');
const output = $('output');
const status = $('status');
const run = $('run');
const stop = $('stop');
const download = $('download');
let compiler;
let executor;
let compilerReady = false;
let busy = false;
let request = 0;
let timer;
let lastWasm;
let compileMs;

const examples = {
    hello: 'using System;\n\nConsole.WriteLine("Hello World!");\n',
    loop: `using System;

int total = 0;
for (int i = 1; i <= 5; i++)
{
    total = total + i;
    Console.WriteLine(total);
}
return total;
`,
    generic: `using System;

Box<int> box = new NumberBox(40);
Console.WriteLine(box.Read());
Console.WriteLine(box is NumberBox);
return box.Read();

class Box<T>
{
    public T Value;
    public Box(T value) { Value = value; }
    public virtual T Read() => Value;
}

class NumberBox : Box<int>
{
    public NumberBox(int value) : base(value) { }
    public override int Read() => base.Read() + 2;
}
`
};

function controls() {
    run.disabled = busy || !compilerReady;
    stop.disabled = !busy;
    download.disabled = !lastWasm;
}

function finish(message) {
    clearTimeout(timer);
    executor?.terminate();
    executor = undefined;
    busy = false;
    status.textContent = message;
    controls();
}

function fail(message) {
    $('error').hidden = false;
    $('error').textContent = message;
    finish('Failed');
}

function startCompiler() {
    compilerReady = false;
    status.textContent = 'Loading compiler…';
    controls();
    const worker = new Worker(new URL('./compiler-worker.js', import.meta.url), { type: 'module' });
    compiler = worker;
    worker.addEventListener('error', event => {
        if (compiler !== worker) return;
        fail(event.message || 'Could not load the compiler. Reload the page to retry.');
    });
    worker.addEventListener('message', ({ data }) => {
        if (compiler !== worker) return;
        if (data.kind === 'ready') {
            compilerReady = true;
            status.textContent = 'Ready';
            controls();
        } else if (data.kind === 'startup-error') {
            fail(data.message);
        } else if (data.id === request && busy) {
            if (data.kind === 'error') fail(data.message);
            else if (data.kind === 'compiled') execute(data);
        }
    });
}

function execute(data) {
    clearTimeout(timer);
    lastWasm = data.wasm;
    compileMs = data.compileMs;
    status.textContent = 'Running…';
    controls();
    const worker = new Worker(new URL('./execute-worker.js', import.meta.url), { type: 'module' });
    executor = worker;
    worker.addEventListener('error', event => {
        if (executor === worker) fail(event.message || 'The execution worker failed.');
    });
    worker.addEventListener('message', ({ data }) => {
        if (data.id !== request || !busy) return;
        if (data.kind === 'output') output.textContent += data.text;
        else if (data.kind === 'error') fail(data.message);
        else if (data.kind === 'done') {
            $('exit-code').textContent = `Exit code ${data.exitCode}`;
            $('metrics').textContent = `${Math.round(compileMs)} ms compile · ${Math.round(data.runMs)} ms run · ${lastWasm.length.toLocaleString()} B`;
            finish('Finished');
        }
    });
    worker.postMessage({ id: request, wasm: lastWasm });
    timer = setTimeout(() => cancel('Stopped after 15 seconds'), 15000);
}

function cancel(message = 'Stopped') {
    if (!busy) return;
    request++;
    const wasCompiling = !executor;
    if (wasCompiling) {
        compiler.terminate();
        compiler = undefined;
    }
    finish(message);
    if (wasCompiling) startCompiler();
}

run.addEventListener('click', () => {
    if (busy || !compilerReady) return;
    busy = true;
    request++;
    lastWasm = undefined;
    output.textContent = '';
    $('error').hidden = true;
    $('error').textContent = '';
    $('exit-code').textContent = '';
    $('metrics').textContent = '';
    status.textContent = 'Compiling…';
    controls();
    compiler.postMessage({ id: request, source: source.value });
    timer = setTimeout(() => cancel('Compilation timed out'), 120000);
});
stop.addEventListener('click', () => cancel());
$('example').addEventListener('change', event => { source.value = examples[event.target.value]; });
download.addEventListener('click', () => {
    if (!lastWasm) return;
    const url = URL.createObjectURL(new Blob([lastWasm], { type: 'application/wasm' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'playground.wasm';
    link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
});
source.addEventListener('keydown', event => {
    if (event.key === 'Tab') {
        event.preventDefault();
        source.setRangeText('    ', source.selectionStart, source.selectionEnd, 'end');
    }
});
document.addEventListener('keydown', event => {
    if ((event.ctrlKey || event.metaKey) && event.key === 'Enter') { event.preventDefault(); run.click(); }
    if (event.key === 'Escape') cancel();
});
startCompiler();
