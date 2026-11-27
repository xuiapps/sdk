import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
const { chromium }=createRequire(import.meta.url)('../../../Playground/browser-tests/node_modules/playwright');
const output=new URL('../../../Playground/browser-tests/test-output/composition/',import.meta.url);
await mkdir(output,{recursive:true});
const browser=await chromium.launch({executablePath:process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH});
try {
    const page=await browser.newPage({viewport:{width:1000,height:1000},deviceScaleFactor:2});
    await page.context().setOffline(true);
    const errors=[];page.on('pageerror',e=>errors.push(String(e)));
    await page.goto(new URL('../../bin/Release/net10.0/TestApp/BasicsExample.html',import.meta.url).href);
    await page.waitForFunction(()=>window.mobileDemo?.ready||window.mobileDemo?.error);
    assert.equal(await page.evaluate(()=>mobileDemo.error),null);
    await page.waitForFunction(()=>{const d=mobileDemo;return d.error||!d.pending&&!d.api.NeedsFrame(d.session);});
    assert.ok(await page.locator('[data-xui-surface]').count()>3);
    await page.screenshot({path:fileURLToPath(new URL('production-basics.png',output))});
    const result=await page.evaluate(()=>{
        const canvas=document.createElement('canvas');canvas.width=canvas.height=400;document.body.append(canvas);
        const host=createCompositionImports(canvas,[]),mount=canvas.nextElementSibling,key={},other={};
        const c=(name,a=0,b=0,c=0,d=0,text='')=>host.Command(name,a,b,c,d,text);
        const begin=(id,bounds,clip=false,translate=0,backdrop=0)=>{
            c('GroupReset',1,clip?1:0,6,backdrop);c('GroupBounds',...bounds);
            c('GroupMatrix',1,0,0,1);c('GroupTranslation',translate,0);return host.Group(id);
        };
        const pixel=(element,x,y)=>Array.from(element.querySelector('canvas').getContext('2d').getImageData(x,y,1,1).data);
        try {
            host.beginFrame(200,200,2);
            c('LinearGradient',0,0,100,0);c('GradientColor',1,0,0,1);c('GradientStop',0);c('GradientColor',0,0,1,1);c('GradientStop',1);c('FillGradient');c('GlobalAlpha',.5);
            begin(key,[0,0,40,40],true,20);c('FillRect',0,0,40,40);host.EndGroup();
            begin(other,[20,60,20,20],false,0,2);c('SetFill',0,1,0,1);c('GlobalAlpha',1);c('FillRect',0,50,80,40);host.EndGroup();
            host.endFrame();
            const first=mount.children[0],second=mount.children[1],secondCanvas=second.querySelector('canvas');
            const inherited=pixel(first,20,20);
            const reference=document.createElement('canvas');reference.width=reference.height=400;
            const r=reference.getContext('2d');r.scale(2,2);
            const gradient=r.createLinearGradient(0,0,100,0);gradient.addColorStop(0,'red');gradient.addColorStop(1,'blue');
            r.fillStyle=gradient;r.globalAlpha=.5;r.save();r.translate(20,0);r.fillRect(0,0,40,40);r.restore();
            const expected=Array.from(r.getImageData(60,20,1,1).data);
            const overflow={left:parseFloat(secondCanvas.style.left),width:parseFloat(secondCanvas.style.width),clip:second.querySelector('[data-xui-content]').style.overflow};
            const blur=second.querySelector('[data-xui-effects]').style.backdropFilter;
            const effectAlpha=second.querySelector('[data-xui-effects]').style.opacity;
            // Native groups enforce the contract's background-before-children order.
            let orderError=false;try{c('FillRect',0,0,1,1);}catch(e){orderError=/overlay group/.test(e.message);}
            host.beginFrame(200,200,2);begin(key,[0,0,40,40],true,40);c('SetFill',1,0,0,1);c('FillRect',0,0,40,40);host.EndGroup();host.endFrame();
            const reused=mount.children[0]===first,pruned=mount.children.length===1,moved=first.style.transform;
            // An arbitrary inherited Canvas clip explicitly chooses immediate drawing.
            host.beginFrame(200,200,2);c('Save');c('BeginPath');c('Rect',0,0,20,20);c('Clip');
            const fallback=begin(key,[0,0,40,40]);c('Save');c('FillRect',0,0,40,40);const ended=host.EndGroup();c('Restore');c('Restore');host.endFrame();
            const flattened=mount.children.length===0;
            host.beginFrame(200,200,2);c('Scale',0,0);begin(key,[0,0,40,40]);c('FillRect',0,0,40,40);host.EndGroup();host.endFrame();
            const collapsed=mount.children[0].querySelector('canvas');
            const collapsedSize=[collapsed.style.display,collapsed.style.width,collapsed.style.height];
            host.beginFrame(200,200,2);begin(key,[0,0,40,40]);host.EndGroup();
            let duplicateError=false;try{begin(key,[0,0,40,40]);}catch(e){duplicateError=/Duplicate sibling/.test(e.message);}
            return {inherited,expected,overflow,blur,effectAlpha,collapsedSize,orderError,reused,pruned,moved,fallback,ended,flattened,duplicateError};
        } finally {host.dispose();canvas.remove();}
    });
    assert.deepEqual(result.inherited,result.expected,'Inherited gradient must match immediate Canvas rendering');
    assert.equal(result.inherited[3],128);
    assert.ok(result.overflow.left<0&&result.overflow.width>=80);assert.equal(result.overflow.clip,'visible');
    assert.match(result.blur,/blur\(18px\)/);
    assert.equal(result.effectAlpha,'0.5');
    assert.deepEqual(result.collapsedSize,['none','0px','0px']);
    for(const flag of ['orderError','reused','pruned','flattened','duplicateError'])assert.equal(result[flag],true,flag);
    assert.match(result.moved,/40, 0\)/);assert.equal(result.fallback,0);assert.equal(result.ended,0);
    assert.deepEqual(errors,[]);
    console.log('DOM composition: keyed reuse, removal, draw ordering, transforms, inherited gradient/alpha, unclipped overflow, blur, and arbitrary-clip fallback passed.');
} finally {await browser.close();}
