// Document state is weakly owned by the WASM document. Dispose releases it immediately.
function createJsonHost() {
    const documents = new WeakMap();
    const node = (owner, index) => documents.get(owner)[index];
    function parse(owner, text, maxDepth, duplicates) {
        try {
            // Delegate complete JSON syntax validation and string decoding to the engine.
            JSON.parse(text);
            const nodes = [];
            let cursor = 0;
            const whitespace = () => { while (cursor < text.length && /[\x20\t\r\n]/.test(text[cursor])) cursor++; };
            function string() {
                const start = cursor++;
                while (text[cursor] !== '"') { if (text[cursor] === '\\') cursor++; cursor++; }
                cursor++;
                return JSON.parse(text.slice(start, cursor));
            }
            function value(depth) {
                whitespace();
                const id = nodes.length, entry = {};
                nodes.push(entry);
                const first = text[cursor];
                if (first === '{' || first === '[') {
                    if (++depth > maxDepth) throw new SyntaxError('Maximum JSON depth exceeded.');
                    entry.kind = first === '{' ? 1 : 2;
                    entry.children = []; entry.properties = new Map();
                    cursor++; whitespace();
                    const end = first === '{' ? '}' : ']';
                    if (text[cursor] !== end) for (;;) {
                        whitespace();
                        const key = first === '{' ? string() : null;
                        if (key !== null) { whitespace(); cursor++; }
                        const child = value(depth);
                        entry.children.push(child);
                        if (key !== null) {
                            if (!duplicates && entry.properties.has(key)) throw new SyntaxError('Duplicate JSON property: ' + key);
                            entry.properties.set(key, child);
                        }
                        whitespace();
                        if (text[cursor] !== ',') break;
                        cursor++;
                    }
                    cursor++;
                } else if (first === '"') { entry.kind = 3; entry.text = string(); }
                else if (first === 't') { entry.kind = 5; cursor += 4; }
                else if (first === 'f') { entry.kind = 6; cursor += 5; }
                else if (first === 'n') { entry.kind = 7; cursor += 4; }
                else {
                    entry.kind = 4;
                    const start = cursor;
                    while (cursor < text.length && /[-+0-9.eE]/.test(text[cursor])) cursor++;
                    entry.raw = text.slice(start, cursor);
                }
                return id;
            }
            value(0);
            documents.set(owner, nodes);
            return null;
        } catch (error) { return String(error.message ?? error); }
    }
    function integerValid(owner, index, bits, unsigned) {
        const raw = node(owner, index).raw;
        if (!/^-?(0|[1-9][0-9]*)$/.test(raw) || unsigned && raw[0] === '-') return 0;
        const value = BigInt(raw), limit = 1n << BigInt(bits - (unsigned ? 0 : 1));
        return +(value >= (unsigned ? 0n : -limit) && value < limit);
    }
    return { imports: Object.freeze({
        parse, close: owner => documents.delete(owner),
        kind: (owner, index) => node(owner, index).kind,
        length: (owner, index) => node(owner, index).children.length,
        element: (owner, index, child) => node(owner, index).children[child],
        property: (owner, index, name) => node(owner, index).properties.get(name) ?? -1,
        string: (owner, index) => node(owner, index).text,
        'valid-string': (owner, index) => {
            const text = node(owner, index).text;
            for (let i = 0; i < text.length; i++) {
                const c = text.charCodeAt(i);
                if (c >= 0xd800 && c <= 0xdbff) {
                    const next = text.charCodeAt(++i);
                    if (!(next >= 0xdc00 && next <= 0xdfff)) return 0;
                } else if (c >= 0xdc00 && c <= 0xdfff) return 0;
            }
            return 1;
        },
        double: (owner, index) => Number(node(owner, index).raw),
        'integer-valid': integerValid,
        i32: (owner, index) => Number(BigInt.asIntN(32, BigInt(node(owner, index).raw))),
        i64: (owner, index) => BigInt.asIntN(64, BigInt(node(owner, index).raw)),
    }) };
}
