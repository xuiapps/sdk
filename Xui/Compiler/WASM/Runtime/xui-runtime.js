// Host operations absent from core WASM. Keep this source shared by browser and Node hosts.
globalThis.xuiRuntimeImports = Object.freeze({
    'xui-string': Object.freeze({
        'int32-to-string': value => value.toString()
    }),
    'xui-math': Object.freeze({
        pow(x, y) {
            // CoreLib follows IEEE/C pow identities; ECMAScript differs for NaN and ±1.
            if (x === 1 || y === 0 || (x === -1 && (y === Infinity || y === -Infinity))) return 1;
            return Math.pow(x, y);
        }
    })
});
