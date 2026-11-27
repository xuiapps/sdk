// Host services for JS strings and weak metadata attached to opaque WASM GC references.
(() => {
    const invalidFormat = [null, 1], unsupportedFormat = [null, 2];
    const enumDescriptors = new Map();
    let graphemes;
    function ordinalUpper(scalar) {
        const code = scalar.codePointAt(0);
        // CoreLib ordinal casing deliberately excludes dotless I and long S.
        if (code === 0x131 || code === 0x17f) return scalar;
        // .NET 10's supplementary ordinal table predates Unicode 17 Beria Erfe.
        if (code >= 0x16ea0 && code <= 0x16ed3) return scalar;
        // Greek iota-subscript has a simple uppercase distinct from JS's expansion.
        if (code >= 0x1f80 && code <= 0x1fa7 && (code & 15) < 8)
            return String.fromCodePoint(code + 8);
        if (code === 0x1fb3 || code === 0x1fc3 || code === 0x1ff3)
            return String.fromCodePoint(code + 9);
        const upper = scalar.toUpperCase();
        // Ordinal comparison uses one-to-one casing, never expansions such as ß -> SS.
        return upper.length === (upper.codePointAt(0) > 0xffff ? 2 : 1) ? upper : scalar;
    }
    function equalsOrdinalIgnoreCase(left, right) {
        if (left === right) return 1;
        if (left === null || right === null || left.length !== right.length) return 0;
        for (let i = 0; i < left.length;) {
            const a = left.codePointAt(i), b = right.codePointAt(i);
            if (a !== b && ordinalUpper(String.fromCodePoint(a)) !== ordinalUpper(String.fromCodePoint(b))) return 0;
            i += a > 0xffff ? 2 : 1;
        }
        return 1;
    }

    // NumberStyles flags shared with System.Globalization. Other valid styles fail explicitly.
    const numberStyle = { leadingWhite:1, trailingWhite:2, sign:4, point:32, group:64, exponent:128, hex:512, binary:1024 };
    function numericWhitespace(text, style) {
        text = text.replace(/\0+$/, '');
        if (style & numberStyle.leadingWhite) text = text.replace(/^[\x09-\x0d ]+/, '');
        if (style & numberStyle.trailingWhite) text = text.replace(/[\x09-\x0d ]+$/, '');
        return text;
    }
    function parseDouble(text, style) {
        if (text === null) return [0,4];
        if (style & ~511) return [0,5];
        if (style & ~231) return [0,2];
        const value = numericWhitespace(text, style);
        const digits = style & numberStyle.group ? '[0-9][0-9,]*' : '[0-9]+';
        const magnitude = style & numberStyle.point ? '(?:'+digits+'(?:\\.[0-9]*)?|\\.[0-9]+)' : digits;
        const expression = '^(?:' + (style & numberStyle.sign ? '[+-]?' : '') + magnitude +
            (style & numberStyle.exponent ? '(?:[eE][+-]?[0-9]+)?' : '') + ')$';
        if (new RegExp(expression).exec(value)?.[0] === value) return [Number(value.replaceAll(',', '')),0];
        // CoreLib accepts invariant special symbols after Unicode trimming even with NumberStyles.None.
        const special = text.replace(/^[\x09-\x0d \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+|[\x09-\x0d \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+$/g, '').toLowerCase();
        if (/^[+-]?nan$/.test(special)) return [NaN,0];
        if (/^[+]?infinity$/.test(special)) return [Infinity,0];
        if (special === '-infinity') return [-Infinity,0];
        return [0,1];
    }
    function parseUInt32(text, style) {
        if (text === null) return [0,4];
        const radix = style & numberStyle.hex ? 16 : style & numberStyle.binary ? 2 : 10;
        if (style & ~2047 || (radix === 16 && style & ~515) || (radix === 2 && style & ~1027)) return [0,5];
        if (radix === 10 && style & ~7) return [0,2];
        const value = numericWhitespace(text, style);
        const pattern = radix === 16 ? /^[0-9a-fA-F]+$/ : radix === 2 ? /^[01]+$/ : style & numberStyle.sign ? /^[+-]?[0-9]+$/ : /^[0-9]+$/;
        if (pattern.exec(value)?.[0] !== value) return [0,1];
        const integer = BigInt((radix === 16 ? '0x' : radix === 2 ? '0b' : '') + value);
        return integer < 0n || integer > 0xffffffffn ? [0,3] : [Number(integer) | 0,0];
    }

    function lowerInvariant(text) {
        // Invariant simple casing is scalar-wise: no final-sigma context and no
        // expansion of dotted capital I into i + combining dot. Lone surrogates survive.
        let result = '';
        for (const scalar of text) result += scalar === '\u0130' ? scalar : scalar.toLowerCase();
        return result;
    }

    function enumFormat(value, descriptor) {
        let type = enumDescriptors.get(descriptor);
        if (!type) {
            type = JSON.parse(descriptor);
            type.values = type.values.map(([bits,name]) => [BigInt(bits),name]);
            enumDescriptors.set(descriptor,type);
        }
        const bits = BigInt.asUintN(type.width,value);
        const exact = type.values.find(([constant]) => constant === bits);
        if (exact) return exact[1];
        if (type.flags && bits !== 0n) {
            let remainder = bits;
            const names = [];
            for (let i=type.values.length-1;i>=0;i--) {
                const [constant,name] = type.values[i];
                if (constant !== 0n && (remainder & constant) === constant) {
                    remainder &= ~constant; names.push(name);
                }
            }
            if (remainder === 0n) return names.reverse().join(', ');
        }
        return (type.signed ? BigInt.asIntN(type.width,bits) : bits).toString();
    }

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

    function fixedCustomFloat(value, digits, single) {
        if (!Number.isFinite(value)) return generalFloat(value, false, false);
        // CLR custom formats start from 15 (double) or 7 (single) significant
        // decimal digits, then round the decimal buffer away from zero. Keep that
        // buffer as an integer so JS binary toFixed rounding cannot change ties.
        const precision = single ? 6 : 14;
        const [mantissa, exponent] = Math.abs(value).toExponential(precision).split('e');
        let decimal = BigInt(mantissa.replace('.', ''));
        const power = Number(exponent) - precision + digits;
        if (power >= 0) decimal *= 10n ** BigInt(power);
        else { const divisor = 10n ** BigInt(-power); decimal = (decimal + divisor / 2n) / divisor; }
        let text = decimal.toString().padStart(digits + 1, '0');
        if (digits) text = text.slice(0, -digits) + '.' + text.slice(-digits);
        return (value < 0 || Object.is(value, -0) ? '-' : '') + text;
    }

    function customFloat(value, spec, single) {
        // Bounded decimal sections: positive;negative;zero, with optional literal signs.
        // Other custom-format syntax deliberately stays unsupported.
        const sections = spec.split(';');
        if (sections.length > 3) return null;
        const patterns = sections.map(section => /^([+-]?)0(?:\.(0*#*))?$/.exec(section));
        if (patterns.some((pattern, i) => !pattern ||
            (sections[i].includes('.') && !pattern[2]?.length) || (pattern[2]?.length ?? 0) > 99)) return null;
        if (!Number.isFinite(value)) return generalFloat(value, false, false);
        let index = sections.length === 1 ? 0 : value === 0 ? (sections.length === 3 ? 2 : 0) : value < 0 ? 1 : 0;
        let pattern = patterns[index], fraction = pattern[2] ?? '';
        let text = fixedCustomFloat(sections.length === 1 ? value : Math.abs(value), fraction.length, single);
        // A section whose precision rounds to zero restarts in the zero/positive section.
        if (sections.length > 1 && Number(text) === 0) {
            index = sections.length === 3 ? 2 : 0;
            pattern = patterns[index]; fraction = pattern[2] ?? '';
            text = fixedCustomFloat(sections.length === 2 && (value < 0 || Object.is(value, -0)) ? -0 : 0, fraction.length, single);
        }
        let optional = fraction.length - fraction.replace(/#+$/, '').length;
        while (optional-- > 0 && text.endsWith('0')) text = text.slice(0, -1);
        if (text.endsWith('.')) text = text.slice(0, -1);
        return text.startsWith('-') ? '-' + pattern[1] + text.slice(1) : pattern[1] + text;
    }

    function scalarFormat(argument, spec) {
        if (argument === null) return ['', 0];
        const { kind, value } = argument;
        if (kind === 0) return unsupportedFormat;
        if (kind === 1) return [value ? 'True' : 'False', 0];
        if (kind === 2) return [String.fromCharCode(value), 0];
        if (kind === 13) return [value, 0];
        if (kind === 7 || kind === 8) {
            const custom = customFloat(value, spec, kind === 7);
            if (custom !== null) return [custom, 0];
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
    // Char.IsWhiteSpace differs from JavaScript \s/trim (notably U+0085 and U+FEFF).
    const whiteSpace = /[\u0009-\u000d\u0020\u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]/;
    const trimSpace = /^[\u0009-\u000d\u0020\u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+|[\u0009-\u000d\u0020\u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+$/g;
    function splitString(text, separator, array, count, options) {
        const trim = value => options & 2 ? value.replace(trimSpace, '') : value;
        const keep = value => !(options & 1) || value.length !== 0;
        if (count === 0) return [];
        if (count === 1) { const value = trim(text); return keep(value) ? [value] : []; }
        const whitespace = array && (separator === null || separator.length === 0);
        const separators = array ? separator : [separator];
        const result = [];
        let start = 0, index = 0;
        while (index <= text.length) {
            let length = 0;
            if (index < text.length) {
                if (whitespace) length = whiteSpace.test(text[index]) ? 1 : 0;
                else for (const value of separators) {
                    if (value && text.startsWith(value, index)) { length = value.length; break; }
                }
                if (!length) { index++; continue; }
            }
            const value = trim(text.slice(start, index));
            if (keep(value)) {
                if (result.length === count - 1) { result.push(trim(text.slice(start))); return result; }
                result.push(value);
            }
            if (index === text.length) break;
            start = index += length;
        }
        return result;
    }
// Host operations absent from core WASM. Keep this source shared by browser and Node hosts.
const monitorDepth = new WeakMap();
const arrayTypes = new WeakMap(), objectHashes = new WeakMap(), stringObjects = new WeakMap();
const stringObjectCleanup = new FinalizationRegistry(({ cache, text, reference }) => {
    if (cache.get(text) === reference) cache.delete(text);
});
let nextObjectHash = 1;
globalThis.xuiRuntimeImports = Object.freeze({
    // One JS agent owns all entries. These operations never call user code or suspend.
    'xui-task': Object.freeze({
        post: (callback, state) => { setTimeout(() => callback(state), 0); }
    }),
    'xui-monitor': Object.freeze({
        change(object, operation) {
            if (object === null || typeof object !== 'object') throw new TypeError('Monitor requires a managed object');
            const depth = monitorDepth.get(object) ?? 0;
            if (operation === 0) { monitorDepth.set(object, depth + 1); return 1; }
            if (operation === 1) {
                if (depth === 0) return 0;
                if (depth === 1) monitorDepth.delete(object); else monitorDepth.set(object, depth - 1);
                return 1;
            }
            if (operation === 2) return depth > 0 ? 1 : 0;
            throw new RangeError('Unsupported monitor operation');
        }
    }),
    'xui-object': Object.freeze({
        'identity-hash': value => {
            if (value === null) return 0;
            let hash = objectHashes.get(value);
            if (hash === undefined) { hash = nextObjectHash; nextObjectHash = (nextObjectHash + 1) | 0; objectHashes.set(value, hash); }
            return hash;
        },
        'register-array': (array, type) => { arrayTypes.set(array, type); },
        'is-array': (array, type) => array !== null && arrayTypes.get(array) === type ? 1 : 0,
    }),
    'xui-string': Object.freeze({
        'object-lookup': (descriptor, text) => stringObjects.get(descriptor)?.get(text)?.deref() ?? null,
        'object-register': (descriptor, text, value) => {
            let cache = stringObjects.get(descriptor);
            if (!cache) { cache = new Map(); stringObjects.set(descriptor, cache); }
            const reference = new WeakRef(value);
            cache.set(text, reference);
            stringObjectCleanup.register(value, { cache, text, reference });
            return value;
        },
        split: splitString,
        'text-element-length': value => {
            graphemes ??= new Intl.Segmenter(undefined, { granularity: 'grapheme' });
            return graphemes.segment(value)[Symbol.iterator]().next().value?.segment.length ?? 0;
        },
        character: value => String.fromCharCode(value),
        vector: length => new Array(length),
        set: (values, index, value) => { values[index] = value; },
        length: values => values.length,
        get: (values, index) => values[index],
        // Decimal's coefficient stays integral; converting through Number loses significant digits.
        'decimal-to-string': (flags, high, low) => {
            const coefficient = (BigInt(high >>> 0) << 64n) | BigInt.asUintN(64, low);
            const scale = (flags >>> 16) & 255;
            let text = coefficient.toString().padStart(scale + 1, '0');
            if (scale) text = text.slice(0, -scale) + '.' + text.slice(-scale);
            return flags < 0 && coefficient !== 0n ? '-' + text : text;
        },
        // A temporary chain of scalar payloads; managed exceptions never escape to JS.
        'exception-frame': (parent, name, message, aggregate) => ({ parent, name, message, aggregate }),
        'exception-finish': frame => {
            let text = null, message = null;
            for (; frame !== null; frame = frame.parent) {
                message = frame.message + (frame.aggregate && message !== null ? ' (' + message + ')' : '');
                const head = frame.name + (message.length ? ': ' + message : '');
                text = head + (text === null ? '' : '\n ---> ' + text + '\n   --- End of inner exception stack trace ---');
            }
            return text;
        },
        'int32-to-string': value => value.toString(),
        'parse-double': parseDouble,
        'parse-uint32': parseUInt32,
        'uint32-to-string': value => (value >>> 0).toString(),
        'int64-to-string': value => value.toString(),
        'uint64-to-string': value => BigInt.asUintN(64, value).toString(),
        'double-to-string': (value, spec) => scalarFormat({kind:8,value}, spec ?? ''),
        'single-to-string': (value, spec) => scalarFormat({kind:7,value:Math.fround(value)}, spec ?? ''),
        'enum-to-string': enumFormat,
        'lower-invariant': lowerInvariant,
        'equals-ordinal-ignore-case': equalsOrdinalIgnoreCase,
        'integer-to-string': (kind, value, spec) => scalarFormat({kind, value: kind === 6 ? BigInt.asUintN(64, value) : value}, spec ?? ''),
        // A callback keeps replacement text literal: JS dollar substitutions are not .NET semantics.
        'replace-ordinal': (text, oldValue, newValue) => text.replaceAll(oldValue, () => newValue ?? ''),
        // StringSearchKind: index, prefix, suffix. JavaScript strings preserve UTF-16 code units.
        'search-ordinal': (text, value, kind) => kind === 0 ? text.indexOf(value) :
            kind === 1 ? +text.startsWith(value) : +text.endsWith(value),
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
    'xui-hash': Object.freeze({
        seed: () => crypto.getRandomValues(new Uint32Array(1))[0]
    }),
    'xui-math': Object.freeze({
        remainder: (x, y) => x % y,
        atan2: Math.atan2,
        cbrt: Math.cbrt,
        sin: Math.sin,
        log: Math.log,
        exp: Math.exp,
        cos: Math.cos,
        tan: Math.tan,
        pow(x, y) {
            // CoreLib follows IEEE/C pow identities; ECMAScript differs for NaN and ±1.
            if (x === 1 || y === 0 || (x === -1 && (y === Infinity || y === -Infinity))) return 1;
            return Math.pow(x, y);
        }
    })
});

})();
