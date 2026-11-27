import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { mkdir, readFile } from 'node:fs/promises';
import { basename } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
const { chromium } = createRequire(import.meta.url)('../../../Playground/browser-tests/node_modules/playwright');
const output = new URL('../../../Playground/browser-tests/test-output/real-catalogue/', import.meta.url);
await mkdir(output, { recursive: true });
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH });
try {
    const page = await browser.newPage({ viewport: { width: 1000, height: 1000 }, deviceScaleFactor: 2, hasTouch: true });
    await page.context().setOffline(true);
    const errors=[];
    page.on('pageerror', error=>errors.push(String(error)));
    page.on('console', message=>{if(message.type()==='error')console.error(message.text());});
    const html = process.argv[2] ? pathToFileURL(process.argv[2]) : new URL('../../bin/Release/net10.0/TestApp/BasicsExample.html', import.meta.url);
    const name = basename(fileURLToPath(html), '.html');
    await page.goto(html.href);
    await page.waitForFunction(()=>window.mobileDemo?.ready || window.mobileDemo?.error);
    assert.equal(await page.evaluate(()=>mobileDemo.error),null);
    assert.ok(await page.locator('[data-xui-surface]').count()>0,'Production pages must create real DOM surfaces');
    const bytes=(await readFile(html)).length;
    assert.equal(await page.locator('#html-size [data-total]').getAttribute('data-total'),String(bytes));
    assert.equal(await page.locator('#html-size tbody tr').evaluateAll(rows=>rows.reduce((sum,row)=>sum+Number(row.dataset.bytes),0)),bytes);
    if(name==='CanvasTestsExample') {
        // Pixel checks isolate the image bridge from carousel layout and artwork changes.
        const result=await page.evaluate(()=>{
            const original=xuiImageAssets;
            const source=document.createElement('canvas'); source.width=2; source.height=2;
            const s=source.getContext('2d'); s.fillStyle='red';s.fillRect(0,0,1,2);s.fillStyle='blue';s.fillRect(1,0,1,2);
            globalThis.xuiImageAssets={'asset://pixels':{width:2,height:2,decoded:source}};
            try {
                const target=document.createElement('canvas');target.width=4;target.height=4;
                const ctx=target.getContext('2d');ctx.imageSmoothingEnabled=false;
                const host=canvasImports(ctx,[]),handle=host.LoadImage('asset://pixels');
                const command=(name,a=0,b=0,c=0,d=0)=>host.Command(name,a,b,c,d,null);
                ctx.globalAlpha=.5;
                command('Image',handle,.5);command('ImageSource',1,0,1,1);command('DrawImage',0,0,4,4);
                const cropped=Array.from(ctx.getImageData(2,2,1,1).data),alpha=ctx.globalAlpha;
                ctx.globalAlpha=1;ctx.clearRect(0,0,4,4);
                command('FillPattern',handle,0);command('FillRect',0,0,4,4);
                const repeated=Array.from(ctx.getImageData(2,3,2,1).data);
                ctx.clearRect(0,0,4,4);command('FillPattern',handle,3);command('FillRect',0,0,4,4);
                const outside=Array.from(ctx.getImageData(3,3,1,1).data);
                return {cropped,alpha,repeated,outside,missing:host.LoadImage('asset://missing'),size:[host.ImageSize(handle,0),host.ImageSize(handle,1)]};
            } finally {globalThis.xuiImageAssets=original;}
        });
        assert.deepEqual(result,{cropped:[0,0,255,64],alpha:.5,repeated:[255,0,0,255,0,0,255,255],outside:[0,0,0,0],missing:0,size:[2,2]});
        assert.ok(await page.locator('#html-size tbody tr').evaluateAll(rows=>Number(rows.find(r=>r.textContent.includes('Image resources')).dataset.bytes)>0));
    }
    const count=await page.evaluate(()=>mobileDemo.api.Sections(mobileDemo.session));
    assert.equal(count, Number(process.argv[3] ?? 3));
    const waitForRendering = async () => {
        const start = await page.evaluate(()=>mobileDemo.api.Frames(mobileDemo.session));
        // Production examples may intentionally animate forever; idle is not an acceptance condition.
        await page.waitForFunction(start=>mobileDemo.error ||
            (!mobileDemo.pending && !mobileDemo.api.NeedsFrame(mobileDemo.session)) ||
            mobileDemo.api.Frames(mobileDemo.session)>=start+90, start);
    };
    for(let section=0;section<count;section++) {
        await page.evaluate(section=>{ const d=mobileDemo; d.api.SetTime(d.session,BigInt(Math.round(performance.now()*10000))); d.api.SelectSection(d.session,section); d.invalidate(); },section);
        await waitForRendering();
        assert.equal(await page.evaluate(()=>mobileDemo.error),null);
        assert.ok(await page.evaluate(()=>mobileDemo.commands.some(c=>c[0]==='FillText')));
        if(name==='CanvasTestsExample' && section>=20) {
            assert.ok(await page.evaluate(()=>xuiImageAssets['asset://test-512.png'].decoded.complete));
            assert.ok(await page.evaluate(command=>mobileDemo.commands.some(c=>c[0]===command),section===20?'DrawImage':'FillPattern'));
        }
        if(name==='HardwareCubeExample') {
            const pixels=()=>page.locator('[data-xui-gpu-surface]').evaluate(canvas=>{
                const bytes=canvas.getContext('2d').getImageData(0,0,canvas.width,canvas.height).data;
                let colored=0,hash=2166136261;
                for(let i=0;i<bytes.length;i+=4) {
                    if(Math.max(bytes[i],bytes[i+1],bytes[i+2])-Math.min(bytes[i],bytes[i+1],bytes[i+2])>40 && bytes[i+3]>200)colored++;
                    hash=Math.imul(hash^bytes[i],16777619);
                }
                return {colored,hash,width:canvas.width,height:canvas.height};
            });
            const before=await pixels();
            assert.ok(before.colored>1000,'The production GPU view must display colored cube faces');
            const resources=await page.evaluate(()=>xuiGpuHost.stats.resources);
            const draws=await page.evaluate(()=>xuiGpuHost.stats.draws);
            const [x,y,k]=await page.evaluate(()=>{
                const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                return [a.ScreenX(s)+a.TargetX(s,'ThreeDSurface')*k,a.ScreenY(s)+a.TargetY(s,'ThreeDSurface')*k,k];
            });
            await page.mouse.move(x,y);await page.mouse.down();
            await page.mouse.move(x+60*k,y+30*k,{steps:20});
            await page.waitForFunction(draws=>mobileDemo.error || xuiGpuHost.stats.draws>draws,draws);
            assert.equal(await page.evaluate(()=>mobileDemo.error),null);
            assert.notEqual((await pixels()).hash,before.hash,'Routed dragging must rotate the real cube');
            await page.mouse.up();await waitForRendering();
            for(const ratio of [1,2,1,2]) {
                await page.evaluate(ratio=>{Object.defineProperty(window,'devicePixelRatio',{configurable:true,value:ratio});mobileDemo.invalidate();},ratio);
                await waitForRendering();
                assert.equal(await page.evaluate(()=>mobileDemo.error),null);
                assert.equal(await page.evaluate(()=>xuiGpuHost.stats.resources),resources,'Resizing must release the old GPU target');
                assert.ok((await pixels()).colored>1000);
            }
        }
        if(name==='TextBoxExample') {
            const focus = async id => {
                const [x,y]=await page.evaluate(id=>{
                    const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                    return [a.ScreenX(s)+a.TargetX(s,id)*k,a.ScreenY(s)+a.TargetY(s,id)*k];
                },id);
                await page.mouse.click(x,y);
                await page.waitForFunction(()=>document.activeElement?.hasAttribute('data-xui-text-input'));
            };
            const value=()=>page.evaluate(()=>{if(mobileDemo.error)throw new Error(mobileDemo.error);return mobileDemo.api.TextValue(mobileDemo.session);});
            await focus('NameBox');
            await page.keyboard.insertText('a\u0301🙂');
            assert.equal(await value(),'a\u0301🙂');
            await page.keyboard.press('Backspace'); assert.equal(await value(),'a\u0301');
            await page.keyboard.press('Backspace'); assert.equal(await value(),'');
            await page.keyboard.type('Alice');
            await page.keyboard.press('Home'); await page.keyboard.press('Delete'); assert.equal(await value(),'lice');
            await page.keyboard.press('ControlOrMeta+A'); await page.keyboard.insertText('Alice');
            await page.keyboard.press('Tab');
            assert.equal(await page.evaluate(()=>mobileDemo.api.TextSecure(mobileDemo.session)),1);
            await page.keyboard.type('secret'); assert.equal(await value(),'secret');
            await focus('NumberBox'); await page.keyboard.press('ControlOrMeta+A');
            await page.keyboard.insertText('12.5'); assert.equal(await value(),'12.5');
            await page.keyboard.insertText('x'); assert.equal(await value(),'12.5');
            await focus('ColorBox'); await page.keyboard.press('ControlOrMeta+A');
            await page.keyboard.insertText('#ABC123'); assert.equal(await value(),'#ABC123');
            await page.keyboard.insertText('G'); assert.equal(await value(),'#ABC123');
            await focus('NameBox'); await page.keyboard.press('End');
            // A committed IME string must enter the actual editor exactly once.
            await page.locator('[data-xui-text-input]').evaluate(input=>{
                input.dispatchEvent(new CompositionEvent('compositionstart',{bubbles:true,data:''}));
                input.dispatchEvent(new CompositionEvent('compositionend',{bubbles:true,data:'Ж'}));
                input.dispatchEvent(new InputEvent('beforeinput',{bubbles:true,cancelable:true,inputType:'insertFromComposition',data:'Ж'}));
            });
            assert.equal(await value(),'AliceЖ');
        }
        if(name==='GesturesExample' && section===3) {
            const before=await page.evaluate(()=>mobileDemo.commands.find(c=>c[0]==='FillText' && /^\d+\.\d°$/.test(c[5]))?.[5]);
            assert.equal(before,'21.0°');
            const [x,y,scale]=await page.evaluate(()=>{
                const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                return [a.ScreenX(s)+a.TargetX(s,'TemperatureDial')*k,a.ScreenY(s)+a.TargetY(s,'TemperatureDial')*k,k];
            });
            await page.mouse.move(x,y);await page.mouse.down();
            await page.mouse.move(x-64*scale,y,{steps:16});
            await page.waitForFunction(before=>mobileDemo.error || mobileDemo.commands.some(c=>c[0]==='FillText' && /^\d+\.\d°$/.test(c[5]) && c[5]!==before),before);
            await page.mouse.up();await waitForRendering();
            assert.equal(await page.evaluate(()=>mobileDemo.error),null);
        }
        if(name==='DesignSystemExample') {
            const clickTarget=async(id,dx=0,dy=0)=>{
                const [x,y]=await page.evaluate(({id,dx,dy})=>{
                    const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                    return [a.ScreenX(s)+(a.TargetX(s,id)+dx)*k,a.ScreenY(s)+(a.TargetY(s,id)+dy)*k];
                },{id,dx,dy});
                await page.mouse.click(x,y); await waitForRendering();
                assert.equal(await page.evaluate(()=>mobileDemo.error),null);
            };
            await clickTarget('DesignSystem.Hue',100);
            await clickTarget('DesignSystem.Tabs',80);
            await clickTarget('DesignSystem.PrimaryAction');
            assert.ok(await page.evaluate(()=>mobileDemo.commands.filter(c=>c[0]==='FillText').map(c=>c[5]).join(' ').includes('Your next adventure is planned.')),
                'The real preview button must update its production status label');
            await page.screenshot({path:fileURLToPath(new URL(`${name}-preview.png`,output))});
            await clickTarget('DesignSystem.Tabs',-80);
        }
        if(name==='MarketExample') {
            const [x,y,k]=await page.evaluate(()=>{
                const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                return [a.ScreenX(s)+a.TargetX(s,'Market.Scroll')*k,a.ScreenY(s)+a.TargetY(s,'Market.Scroll')*k,k];
            });
            assert.ok(await page.evaluate(()=>mobileDemo.commands.some(c=>c[0]==='FillText' && /^[+-]\d+\.\d{2}%$/.test(c[5]))),
                'The real market list must render signed percentage changes');
            await page.screenshot({path:fileURLToPath(new URL(`${name}-list.png`,output))});
            await page.mouse.click(x,y);await waitForRendering();
            assert.ok(await page.evaluate(()=>mobileDemo.api.HasView(mobileDemo.session,'Market.Detail')),
                'Selecting a real stock must push its production detail page');
            assert.ok(await page.evaluate(()=>mobileDemo.commands.some(c=>c[0]==='FillText' && c[5]==='PRICE HISTORY')));
            await page.screenshot({path:fileURLToPath(new URL(`${name}-detail.png`,output))});
            await page.mouse.move(x,y+100*k);await page.mouse.down();
            await page.mouse.move(x,y-180*k,{steps:20});await page.mouse.up();await waitForRendering();
            assert.equal(await page.evaluate(()=>mobileDemo.error),null);
            const [bx,by]=await page.evaluate(()=>{
                const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                return [a.ScreenX(s)+a.TargetX(s,'Back')*k,a.ScreenY(s)+a.TargetY(s,'Back')*k];
            });
            await page.mouse.click(bx,by);await waitForRendering();
            assert.ok(await page.evaluate(()=>mobileDemo.api.HasView(mobileDemo.session,'Market.Grid')));
            const symbols=()=>page.evaluate(()=>mobileDemo.commands.filter(c=>c[0]==='FillText' && /^[A-Z]{4}$/.test(c[5])).map(c=>c[5]));
            const before=await symbols();
            await page.mouse.move(x,y+150*k);await page.mouse.down();
            await page.mouse.move(x,y-150*k,{steps:20});await page.mouse.up();await waitForRendering();
            assert.notDeepEqual(await symbols(),before,'Scrolling must render a different slice of the 5,000-row dataset');
        }
        if(name==='LiveVibrationExample' || name==='RecordedVibrationExample') {
            const recorded=name==='RecordedVibrationExample';
            const target=async id=>page.evaluate(id=>{
                const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                return [a.ScreenX(s)+a.TargetX(s,id)*k,a.ScreenY(s)+a.TargetY(s,id)*k,k];
            },id);
            const [x,y,k]=await target(recorded?'Vibration.Record.Pad':'Vibration.Live.Pad');
            const reading=()=>page.evaluate(()=>mobileDemo.commands.find(c=>c[0]==='FillText' && c[5]?.startsWith('Strength '))?.[5]);
            await page.mouse.move(x-30*k,y+25*k);await page.mouse.down();
            await waitForRendering(); const before=await reading();
            await page.mouse.move(x+30*k,y-25*k,{steps:16});await waitForRendering();
            assert.notEqual(await reading(),before,'The production pad must update strength/frequency while dragging');
            await page.mouse.up();await waitForRendering();
            assert.equal(await page.evaluate(()=>mobileDemo.error),null);
            if(recorded) {
                assert.ok(await page.evaluate(()=>mobileDemo.api.HasView(mobileDemo.session,'Vibration.Record.Playback')));
                assert.ok(await page.evaluate(()=>mobileDemo.commands.some(c=>c[0]==='FillText' && c[5]==='Stop')),
                    'A recorded pattern must begin looping after release');
                const [bx,by]=await target('Vibration.Record.Playback');
                await page.mouse.click(bx,by);await waitForRendering();
                assert.ok(await page.evaluate(()=>mobileDemo.commands.some(c=>c[0]==='FillText' && c[5]==='Start')));
                await page.mouse.click(bx,by);await waitForRendering();
                assert.ok(await page.evaluate(()=>mobileDemo.commands.some(c=>c[0]==='FillText' && c[5]==='Stop')));
            } else assert.ok((await reading()).startsWith('Strength 0.00'), 'Release stops the live pad');
            assert.ok(await page.evaluate(()=>mobileDemo.commands.some(c=>c[0]==='FillText' && c[5].includes('no vibration hardware'))));
        }
        if(name==='HapticsExample') {
            // Exercise the production buttons with the platform service absent.
            // This verifies the nullable service path, not hardware vibration.
            for(const id of ['Haptic.Tick','Haptic.LightImpact','Haptic.MediumImpact']) {
                const [x,y]=await page.evaluate(id=>{
                    const d=mobileDemo,a=d.api,s=d.session,k=a.ScreenScale(s);
                    return [a.ScreenX(s)+a.TargetX(s,id)*k,a.ScreenY(s)+a.TargetY(s,id)*k];
                },id);
                await page.mouse.click(x,y);await waitForRendering();
                assert.equal(await page.evaluate(()=>mobileDemo.error),null);
            }
        }
        await page.screenshot({ path:fileURLToPath(new URL(`${name}-${section}.png`,output)) });
    }
    // Actual pointer routing must keep the production carousel usable after direct section selection.
    await page.mouse.move(600,550); await page.mouse.down();
    await page.mouse.move(400,550,{steps:16}); await page.mouse.up();
    await waitForRendering();
    assert.equal(await page.evaluate(()=>mobileDemo.error),null);
    assert.ok(await page.evaluate(()=>mobileDemo.api.PointerEvents(mobileDemo.session)>0));
    assert.deepEqual(errors,[]);
    if(name==='HardwareCubeExample') {
        await page.evaluate(()=>{mobileDemo.api.DropContent(mobileDemo.session);mobileDemo.invalidate();});
        await waitForRendering();
        assert.equal(await page.evaluate(()=>mobileDemo.error),null);
        assert.equal(await page.evaluate(()=>xuiGpuHost.stats.resources),1,'Removing the view releases its commands, shaders, and retained target; only the device remains');
    }
    await page.evaluate(()=>mobileDemo.dispose());
    if(name==='HardwareCubeExample') assert.equal(await page.evaluate(()=>xuiGpuHost.stats.resources),0);
    if(name==='TextBoxExample') assert.equal(await page.locator('[data-xui-text-input]').count(),0,'Disposal removes the native input bridge');
    console.log(`Production ${name}: ${count} sections, offline rendering and pointer routing passed.`);
} finally { await browser.close(); }
