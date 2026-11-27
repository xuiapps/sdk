import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
import {readFile} from 'node:fs/promises';
const {chromium}=createRequire(import.meta.url)('../../../Playground/browser-tests/node_modules/playwright');
const host=await readFile(new URL('BrowserGpuHost.js',import.meta.url),'utf8');
const shaders=JSON.parse(await readFile(new URL('../../bin/Release/net10.0/Gpu/shaders.json',import.meta.url),'utf8'));
const browser=await chromium.launch({executablePath:process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH});
try {
    const page=await browser.newPage();await page.context().setOffline(true);await page.setContent('<canvas width="128" height="128"></canvas>');
    await page.addScriptTag({content:host});
    const result=await page.evaluate(source=>{
        const memory=new WebAssembly.Memory({initial:1});
        const host=createGpuHost(()=>memory),invoke=(op,id,a=0,b=0,c=0,d=0,text='')=>host.imports.Invoke(op,id,a,b,c,d,text);
        const device=invoke('Device',0),vs=invoke('VertexShader',device,0,0,0,0,source),fs=invoke('FragmentShader',device,0,0,0,0,source);
        const target=invoke('Target',device,128,128),commands=invoke('Commands',device);
        new Float32Array(memory.buffer,0,21).set([-.8,-.8,.5,1,0,0,1, .8,-.8,.5,1,0,0,1, 0,.8,.5,1,0,0,1]);
        new Float32Array(memory.buffer,256,16).set([.3,0,0,.55,0,.3,0,.2,0,0,1,0,0,0,0,1]);
        const float=new DataView(new ArrayBuffer(4));float.setFloat32(0,1,true);const one=float.getInt32(0,true);
        const counts=[];
        for(let i=0;i<20;i++) {
            invoke('Begin',commands,target);invoke('Clear',commands,0,0,0,one);
            invoke('Pipeline',commands,vs,fs,3,0);invoke('Attribute',commands,0,3);invoke('Attribute',commands,1,4);
            invoke('Vertices',commands,0,84,28,3);invoke('Constants',commands,256,64);
            invoke('Draw',commands,3);invoke('End',commands);invoke('Commit',commands);
            host.present(target,document.querySelector('canvas').getContext('2d'),0,0,128,128);counts.push(host.stats.resources);
        }
        const context=document.querySelector('canvas').getContext('2d');
        const center=Array.from(context.getImageData(99,51,1,1).data),origin=Array.from(context.getImageData(64,64,1,1).data);
        const fails=action=>{try{action();return false;}catch{return true;}};
        const rangeRejected=fails(()=>invoke('Vertices',commands,memory.buffer.byteLength-10,84,28,3));
        invoke('Dispose',vs);const disposedRejected=fails(()=>{invoke('Begin',commands,target);invoke('Pipeline',commands,vs,fs,3,0);});
        host.dispose();const released=host.stats.resources;
        return {center,origin,counts,rangeRejected,disposedRejected,released,draws:host.stats.draws};
    },shaders.CubeShader);
    assert.deepEqual(result.center,[255,0,0,255]);assert.deepEqual(result.origin,[0,0,0,255]);
    assert.ok(result.counts.every(n=>n===5));assert.equal(result.draws,20);
    assert.equal(result.rangeRejected,true);assert.equal(result.disposedRejected,true);assert.equal(result.released,0);
    console.log(JSON.stringify(result));
} finally {await browser.close();}
