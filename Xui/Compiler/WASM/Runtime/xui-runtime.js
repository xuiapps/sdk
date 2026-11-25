// Host operations absent from core WASM. Keep this source shared by browser and Node hosts.
globalThis.xuiRuntimeImports = Object.freeze({
    'xui-string': Object.freeze({
        'int32-to-string': value => value.toString(),
        // FNV-1a over UTF-16 code units; consistent with ordinal equality, not CoreLib's randomized hash.
        'ordinal-hash': value => {
            if (value === null) return 0;
            let hash = 0x811c9dc5;
            for (let i = 0; i < value.length; i++) hash = Math.imul(hash ^ value.charCodeAt(i), 0x01000193);
            return hash | 0;
        }
    }),
    'xui-math': Object.freeze({
        pow(x, y) {
            // CoreLib follows IEEE/C pow identities; ECMAScript differs for NaN and ±1.
            if (x === 1 || y === 0 || (x === -1 && (y === Infinity || y === -Infinity))) return 1;
            return Math.pow(x, y);
        }
    })
});
