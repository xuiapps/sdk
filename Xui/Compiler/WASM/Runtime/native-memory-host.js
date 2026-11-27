// Each host owns one module's heap. The initial memory contains the stack and PE data.
function createNativeMemoryHost(getMemory, {maximumBytes = 0x80000000} = {}) {
    const allocated = new Map(), free = [];
    const stats = { allocations: 0, frees: 0, liveBytes: 0, peakBytes: 0, grows: 0 };
    let memory, end;
    function bind() {
        const current = getMemory();
        if (!memory) { memory = current; end = memory.buffer.byteLength; }
        else if (memory !== current) throw new Error('A native heap cannot be shared between module instances.');
    }
    function allocate(count, size, zero) {
        bind(); count >>>= 0; size >>>= 0;
        if (size !== 0 && count > Math.floor(maximumBytes / size)) return 0;
        const bytes = Math.max(16, Math.ceil(count * size / 16) * 16);
        let address, index = free.findIndex(block => block.bytes >= bytes);
        if (index >= 0) {
            const block = free[index]; address = block.address;
            if (block.bytes === bytes) free.splice(index, 1);
            else { block.address += bytes; block.bytes -= bytes; }
        } else {
            if (end > maximumBytes - bytes) return 0;
            const required = end + bytes;
            if (required > memory.buffer.byteLength) {
                try { memory.grow(Math.ceil((required - memory.buffer.byteLength) / 65536)); }
                catch (error) { if (error instanceof RangeError) return 0; throw error; }
                stats.grows++;
            }
            address = end; end = required;
        }
        allocated.set(address, bytes);
        if (zero) new Uint8Array(memory.buffer, address, bytes).fill(0);
        stats.allocations++; stats.liveBytes += bytes;
        stats.peakBytes = Math.max(stats.peakBytes, stats.liveBytes);
        return address;
    }
    function release(pointer) {
        pointer >>>= 0;
        if (pointer === 0) return;
        bind();
        const bytes = allocated.get(pointer);
        if (bytes === undefined) throw new Error('Invalid native free: ' + pointer);
        allocated.delete(pointer); stats.frees++; stats.liveBytes -= bytes;
        let index = free.findIndex(block => block.address > pointer);
        if (index < 0) index = free.length;
        free.splice(index, 0, {address: pointer, bytes});
        if (index > 0 && free[index-1].address + free[index-1].bytes === pointer) {
            free[index-1].bytes += bytes; free.splice(index, 1); index--;
        }
        if (index + 1 < free.length && free[index].address + free[index].bytes === free[index+1].address) {
            free[index].bytes += free[index+1].bytes; free.splice(index+1, 1);
        }
        // Keep committed pages, but reclaim trailing address space for subsequent allocations.
        const last = free[free.length-1];
        if (last && last.address + last.bytes === end) { end = last.address; free.pop(); }
    }
    return {imports: Object.freeze({allocate, free: release}), stats};
}
