// FormattingValueKind is the scalar ABI; WASM GC references never cross this boundary.
(() => {
    const invalidFormat = [null, 1], unsupportedFormat = [null, 2];

    function generalFloat(value, single, lower) {
        if (Number.isNaN(value)) return 'NaN';
        if (!Number.isFinite(value)) return value < 0 ? '-Infinity' : 'Infinity';
        if (Object.is(value, -0)) return '-0';
        if (value === 0) return '0';
        let text = value.toString();
        if (single) {
            for (let digits = 1; digits <= 9; digits++) {
                const rounded = value.toPrecision(digits);
                if (Math.fround(Number(rounded)) === value) { text = Number(rounded).toString(); break; }
            }
        }
        const number = Number(text), exponent = Number(number.toExponential().split('e')[1]);
        if (exponent < -4 || exponent >= (single ? 9 : 17)) {
            if (!/[eE]/.test(text)) text = number.toExponential();
            const [mantissa, power] = text.toLowerCase().split('e');
            return mantissa + (lower ? 'e' : 'E') + (Number(power) < 0 ? '-' : '+') + Math.abs(Number(power)).toString().padStart(2, '0');
        }
        if (/[eE]/.test(text)) text = number.toFixed(Math.max(0, -exponent + text.split('e')[0].replace(/[-.]/g, '').length - 1));
        return text;
    }

    function scalarFormat(argument, spec) {
        if (argument === null) return ['', 0];
        const { kind, value } = argument;
        if (kind === 0) return unsupportedFormat;
        if (kind === 1) return [value ? 'True' : 'False', 0];
        if (kind === 2) return [String.fromCharCode(value), 0];
        if (kind === 13) return [value, 0];
        if (kind === 7 || kind === 8) {
            if (spec !== '' && !/^[gGrR]$/.test(spec)) return unsupportedFormat;
            return [generalFloat(value, kind === 7, spec === 'g' || spec === 'r'), 0];
        }
        if (spec === '' || /^[gGdD]$/.test(spec)) return [value.toString(), 0];
        const match = /^([dDxX])([0-9]*)$/.exec(spec);
        if (!match) return /^[a-zA-Z][0-9]*$/.test(spec) && !/^[cCeEfFgGnNpPbB]/.test(spec) ? invalidFormat : unsupportedFormat;
        const width = match[2] === '' ? 0 : Number(match[2]);
        if (width > 999999999) return invalidFormat;
        if (match[1].toLowerCase() === 'd') {
            const negative = value < 0;
            return [(negative ? '-' : '') + (negative ? -value : value).toString().padStart(width, '0'), 0];
        }
        // Decimal formatting keeps Int64/UInt64 as BigInt; hexadecimal uses target-width bits.
        const bits = kind === 5 || kind === 6 ? 64 : kind === 9 || kind === 11 ? 8 : kind === 10 || kind === 12 ? 16 : 32;
        let text = BigInt.asUintN(bits, BigInt(value)).toString(16).padStart(width, '0');
        return [match[1] === 'X' ? text.toUpperCase() : text, 0];
    }

    function compositeFormat(format, args) {
        let result = '', position = 0;
        const digit = c => c >= '0' && c <= '9';
        while (position < format.length) {
            const c = format[position++];
            if (c === '}') {
                if (format[position++] !== '}') return invalidFormat;
                result += '}'; continue;
            }
            if (c !== '{') { result += c; continue; }
            if (format[position] === '{') { position++; result += '{'; continue; }
            if (!digit(format[position])) return invalidFormat;
            let index = 0;
            do { index = index * 10 + Number(format[position++]); } while (digit(format[position]) && index < 1000000);
            if (index >= args.length) return invalidFormat;
            while (format[position] === ' ') position++;
            let width = 0, left = false;
            if (format[position] === ',') {
                position++;
                while (format[position] === ' ') position++;
                if (format[position] === '-') { left = true; position++; }
                if (!digit(format[position])) return invalidFormat;
                do { width = width * 10 + Number(format[position++]); } while (digit(format[position]) && width < 1000000);
                while (format[position] === ' ') position++;
            }
            let spec = '';
            if (format[position] === ':') {
                const start = ++position;
                while (position < format.length && format[position] !== '}') {
                    if (format[position] === '{') return invalidFormat;
                    position++;
                }
                spec = format.slice(start, position);
            }
            if (format[position++] !== '}') return invalidFormat;
            const [text, error] = scalarFormat(args[index], spec);
            if (error) return [null, error];
            result += left ? text.padEnd(width, ' ') : text.padStart(width, ' ');
        }
        return [result, 0];
    }
// Host operations absent from core WASM. Keep this source shared by browser and Node hosts.
globalThis.xuiRuntimeImports = Object.freeze({
    'xui-string': Object.freeze({
        'int32-to-string': value => value.toString(),
        'format-text': value => value === null ? null : ({ kind: 13, value }),
        'format-start': count => new Array(count),
        'format-set': (args, index, value) => { args[index] = value; },
        'format-i32': (kind, value) => ({ kind, value: kind === 4 ? value >>> 0 : value }),
        'format-i64': (kind, value) => ({ kind, value: kind === 6 ? BigInt.asUintN(64, value) : value }),
        'format-f64': (kind, value) => ({ kind, value }),
        'format-finish': compositeFormat,
        // FNV-1a over UTF-16 code units; consistent with ordinal equality, not CoreLib's randomized hash.
        'ordinal-hash': value => {
            if (value === null) return 0;
            let hash = 0x811c9dc5;
            for (let i = 0; i < value.length; i++) hash = Math.imul(hash ^ value.charCodeAt(i), 0x01000193);
            return hash | 0;
        }
    }),
    'xui-clock': Object.freeze({
        // Milliseconds since this host's time origin, not wall time or machine boot time.
        'tick-count64': () => BigInt(Math.floor(performance.now())),
        'timestamp': () => BigInt(Math.floor(performance.now() * 1_000_000))
    }),
    'xui-math': Object.freeze({
        remainder: (x, y) => x % y,
        sin: Math.sin,
        cos: Math.cos,
        pow(x, y) {
            // CoreLib follows IEEE/C pow identities; ECMAScript differs for NaN and ±1.
            if (x === 1 || y === 0 || (x === -1 && (y === Infinity || y === -Infinity))) return 1;
            return Math.pow(x, y);
        }
    })
});

})();
