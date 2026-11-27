import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { readFile, mkdir } from 'node:fs/promises';
const { chromium } = createRequire(import.meta.url)('../../../Playground/browser-tests/node_modules/playwright');
const shaders = JSON.parse(await readFile(new URL('../../bin/Release/net10.0/Gpu/shaders.json', import.meta.url), 'utf8'));
const browser = await chromium.launch({ executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH });
try {
    const page = await browser.newPage({ viewport: { width: 300, height: 160 } });
    await page.context().setOffline(true);
    await page.setContent('<canvas width="128" height="128"></canvas>');
    const result = await page.evaluate(shaders => {
        const gl = document.querySelector('canvas').getContext('webgl2', { antialias: false, preserveDrawingBuffer: true });
        if (!gl) throw new Error('WebGL 2 is required');
        const compile = (source, vertex) => {
            const shader = gl.createShader(vertex ? gl.VERTEX_SHADER : gl.FRAGMENT_SHADER);
            gl.shaderSource(shader, source.replace('#version 300 es', '#version 300 es\n#define ' + (vertex ? 'XUI_VERTEX' : 'XUI_FRAGMENT')));
            gl.compileShader(shader);
            if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) throw new Error(gl.getShaderInfoLog(shader));
            return shader;
        };
        const link = source => {
            const program = gl.createProgram();
            const vertex = compile(source, true), fragment = compile(source, false);
            gl.attachShader(program, vertex);gl.attachShader(program, fragment);gl.linkProgram(program);
            if (!gl.getProgramParameter(program, gl.LINK_STATUS)) throw new Error(gl.getProgramInfoLog(program));
            gl.deleteShader(vertex);gl.deleteShader(fragment);
            return program;
        };
        const programs = Object.fromEntries(Object.entries(shaders).map(([name, source]) => [name, link(source)]));
        const uniforms = (program, values) => {
            const block = gl.getUniformBlockIndex(program, 'XuiBindings');
            const bytes = gl.getActiveUniformBlockParameter(program, block, gl.UNIFORM_BLOCK_DATA_SIZE);
            if (bytes !== values.length * 4) throw new Error(`Unexpected uniform layout ${bytes}`);
            gl.uniformBlockBinding(program, block, 0);
            const buffer = gl.createBuffer();gl.bindBuffer(gl.UNIFORM_BUFFER, buffer);
            gl.bufferData(gl.UNIFORM_BUFFER, new Float32Array(values), gl.STATIC_DRAW);gl.bindBufferBase(gl.UNIFORM_BUFFER, 0, buffer);
            return buffer;
        };
        const vertices = (values, widths) => {
            const vao = gl.createVertexArray();gl.bindVertexArray(vao);
            const buffer = gl.createBuffer();gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(values), gl.STATIC_DRAW);
            const stride = widths.reduce((a,b)=>a+b,0)*4;let offset=0;
            widths.forEach((width,index)=>{gl.enableVertexAttribArray(index);gl.vertexAttribPointer(index,width,gl.FLOAT,false,stride,offset);offset+=width*4;});
            return {vao,buffer};
        };
        const pixel = (x,y) => {const bytes=new Uint8Array(4);gl.readPixels(x,y,1,1,gl.RGBA,gl.UNSIGNED_BYTE,bytes);return Array.from(bytes);};
        gl.viewport(0,0,128,128);gl.clearColor(0,0,0,1);
        const cube = programs.CubeShader;gl.useProgram(cube);
        uniforms(cube, [.3,0,0,.55, 0,.3,0,.2, 0,0,1,0, 0,0,0,1]);
        const indices = gl.getUniformIndices(cube,['x_bindings.x_MVP']);
        const rowMajor = gl.getActiveUniforms(cube, indices, gl.UNIFORM_IS_ROW_MAJOR)[0];
        const stride = gl.getActiveUniforms(cube, indices, gl.UNIFORM_MATRIX_STRIDE)[0];
        const drawTriangle = depth => {
            gl.clear(gl.COLOR_BUFFER_BIT);
            const {vao,buffer}=vertices([-.8,-.8,depth,1,0,0,1, .8,-.8,depth,1,0,0,1, 0,.8,depth,1,0,0,1],[3,4]);
            gl.drawArrays(gl.TRIANGLES,0,3);
            const result={center:pixel(99,76),origin:pixel(64,64)};
            gl.deleteBuffer(buffer);gl.deleteVertexArray(vao);return result;
        };
        const cubePixels=drawTriangle(.5),nearClipped=drawTriangle(-.1),farClipped=drawTriangle(1.1);
        const floorplan=programs.FloorplanShader;gl.useProgram(floorplan);
        uniforms(floorplan,[1,1,0,0, .5,1,0,0]);
        vertices([-1,-1,0,0, 0,0,0,1, 1,-1,1,0, 0,0,0,1, 0,1,.5,1, 0,0,0,1],[4,4]);
        const texture=gl.createTexture();gl.activeTexture(gl.TEXTURE0);gl.bindTexture(gl.TEXTURE_2D,texture);
        gl.texImage2D(gl.TEXTURE_2D,0,gl.RGBA8,2,2,0,gl.RGBA,gl.UNSIGNED_BYTE,new Uint8Array([255,0,0,255,255,0,0,255,255,0,0,255,255,0,0,255]));
        gl.texImage2D(gl.TEXTURE_2D,1,gl.RGBA8,1,1,0,gl.RGBA,gl.UNSIGNED_BYTE,new Uint8Array([40,80,120,200]));
        gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MIN_FILTER,gl.NEAREST_MIPMAP_NEAREST);gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MAG_FILTER,gl.NEAREST);
        gl.uniform1i(gl.getUniformLocation(floorplan,'x_texture_Atlas_Sampler'),0);
        gl.clear(gl.COLOR_BUFFER_BIT);gl.drawArrays(gl.TRIANGLES,0,3);
        const sampled=pixel(64,64),error=gl.getError();
        return {linked:Object.keys(programs),rowMajor:Boolean(rowMajor),stride,cubePixels,nearClipped,farClipped,sampled,error};
    }, shaders);
    assert.deepEqual(result.linked, ['CubeShader','FloorplanShader']);
    assert.equal(result.rowMajor,true);assert.equal(result.stride,16);
    assert.deepEqual(result.cubePixels,{center:[255,0,0,255],origin:[0,0,0,255]});
    for (const clipped of [result.nearClipped,result.farClipped]) assert.deepEqual(clipped.center,[0,0,0,255]);
    for (const [index,expected] of [20,40,60,100].entries()) assert.ok(Math.abs(result.sampled[index]-expected)<=1,JSON.stringify(result.sampled));
    assert.equal(result.error,0);
    const output=new URL('../../../Playground/browser-tests/test-output/shaders/',import.meta.url);await mkdir(output,{recursive:true});
    await page.screenshot({path:new URL('floorplan-sampling.png',output).pathname});
    console.log(JSON.stringify(result));
} finally { await browser.close(); }
