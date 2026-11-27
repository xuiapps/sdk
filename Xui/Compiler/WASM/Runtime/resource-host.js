// Preparation may await browser decompression. All subsequent stream operations are synchronous.
async function createResourceHost(catalogue, getMemory) {
    const assets = new Map(), streams = new WeakMap();
    const stats = { opens:0, closes:0, reads:0, bytes:0 };
    await Promise.all(catalogue.map(async entry => {
        const binary = atob(entry.data), bytes = new Uint8Array(binary.length);
        for(let i=0;i<bytes.length;i++) bytes[i]=binary.charCodeAt(i);
        const asset = { bytes, gzip:entry.gzip, decoded:null, error:false };
        if(entry.gzip) {
            try {
                const output = new Blob([bytes]).stream().pipeThrough(new DecompressionStream('gzip'));
                asset.decoded = new Uint8Array(await new Response(output).arrayBuffer());
            } catch { asset.error=true; }
        }
        let assembly=assets.get(entry.assembly);
        if(!assembly) assets.set(entry.assembly,assembly=new Map());
        if(assembly.has(entry.name)) throw new Error('Duplicate manifest resource: '+entry.name);
        assembly.set(entry.name,asset);
    }));
    const state = stream => {
        const value=streams.get(stream);
        if(!value) throw new Error('Invalid resource stream host state');
        return value;
    };
    const imports = Object.freeze({
        open(stream,assembly,name) {
            const asset=assets.get(assembly)?.get(name);
            if(!asset)return 0;
            streams.set(stream,{asset,data:asset.bytes,position:0,error:false});stats.opens++;return 1;
        },
        gzip(target,source) {
            const input=streams.get(source);
            if(!input || !input.asset.gzip || input.position!==0 || input.data!==input.asset.bytes)return 0;
            streams.set(target,{asset:input.asset,data:input.asset.decoded,position:0,error:input.asset.error,source:input});
            stats.opens++;return 1;
        },
        read(stream,destination,count) {
            const input=state(stream);stats.reads++;
            if(count===0)return 0;
            if(input.source)input.source.position=input.source.data.length;
            if(input.error)return -1;
            const length=Math.min(count,Math.max(0,input.data.length-input.position));
            // Reacquire the view for every call: memory.grow can replace its buffer.
            const memory=new Uint8Array(getMemory().buffer);
            memory.set(input.data.subarray(input.position,input.position+length),destination>>>0);
            input.position+=length;stats.bytes+=length;return length;
        },
        length:stream=>BigInt(state(stream).data.length),
        position:stream=>BigInt(state(stream).position),
        seek:(stream,position)=>{state(stream).position=Number(position);},
        close(stream){if(streams.delete(stream))stats.closes++;}
    });
    return Object.freeze({imports,stats});
}
