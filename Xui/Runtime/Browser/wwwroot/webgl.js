// Adapted from the compiler browser GPU acceptance host. Upload views never escape a call.
export function createGpuHost() {
    const recordOnly = false;
    let scratch = new Uint8Array(4096), renderer = "unavailable";
    const resources = new Map();let next=1,draws=0,uploads=0;
    const floatBits=new DataView(new ArrayBuffer(4));
    const float=value=>{floatBits.setInt32(0,value,true);return floatBits.getFloat32(0,true);};
    const add=value=>{const handle=next++;resources.set(handle,value);return handle;};
    const get=(handle,kind)=>{const value=resources.get(handle);if(!value || kind&&value.kind!==kind)throw new Error(`Invalid GPU ${kind??'resource'} handle ${handle}`);return value;};
    function compile(device,source,vertex) {
        if(!source.startsWith('#version 300 es'))throw new Error('GPU shader requires GLSL ES 3.00');
        if(recordOnly)return null;
        const gl=device.gl,shader=gl.createShader(vertex?gl.VERTEX_SHADER:gl.FRAGMENT_SHADER);
        gl.shaderSource(shader,source.replace('#version 300 es','#version 300 es\n#define '+(vertex?'XUI_VERTEX':'XUI_FRAGMENT')));gl.compileShader(shader);
        if(!gl.getShaderParameter(shader,gl.COMPILE_STATUS)){const message=gl.getShaderInfoLog(shader);gl.deleteShader(shader);throw new Error(message);}
        return shader;
    }
    function release(handle) {
        if(!handle)return;
        const r=get(handle),gl=r.device?.gl??r.gl;
        if(r.kind==='device') {
            for(const [id,child] of [...resources])if(child.device===r)release(id);
            for(const p of r.programs.values())gl?.deleteProgram(p.program);
            r.programs.clear();
        } else if(r.kind==='target') {gl?.deleteFramebuffer(r.framebuffer);gl?.deleteTexture(r.texture);gl?.deleteRenderbuffer(r.depth);}
        else if(r.kind==='commands') {gl?.deleteBuffer(r.vertices);gl?.deleteBuffer(r.constants);gl?.deleteVertexArray(r.vao);for(const sampler of r.samplers?.values()??[])gl?.deleteSampler(sampler);}
        else if(r.kind==='buffer')gl?.deleteBuffer(r.buffer);
        else if(r.kind==='texture')gl?.deleteTexture(r.texture);
        else if(r.kind==='vertex'||r.kind==='fragment') {
            for(const [key,p] of r.device.programs)if(p.vertex===handle||p.fragment===handle){gl?.deleteProgram(p.program);r.device.programs.delete(key);}
            gl?.deleteShader(r.shader);
        }
        resources.delete(handle);
    }
    function invoke(op,handle,a,b,c,d,text) {
        if(op===0) {
            let canvas=null,gl=null;
            if(!recordOnly){canvas=new OffscreenCanvas(1,1);gl=canvas.getContext('webgl2',{alpha:true,antialias:false,preserveDrawingBuffer:true,powerPreference:'high-performance'});if(!gl)return 0;
                renderer='WebGL 2';}
            return add({kind:'device',canvas,gl,programs:new Map()});
        }
        if(op===1){release(handle);return 0;}
        const r=get(handle),device=r.kind==='device'?r:r.device,gl=device.gl;
        if(gl.isContextLost())throw new Error('WebGL context lost. Reload to recreate GPU resources.');
        const owned=(id,kind)=>{const value=get(id,kind);if(value.device!==device)throw new Error('GPU resource belongs to another device');return value;};
        switch(op) {
            case 2:case 3:
                get(handle,'device');return add({kind:op===2?'vertex':'fragment',device,shader:compile(device,text,op===2)});
            case 4:
                get(handle,'device');return add({kind:'commands',device,vao:gl?.createVertexArray(),vertices:gl?.createBuffer(),constants:gl?.createBuffer(),attributes:[],samplers:new Map(),inPass:false});
            case 13: {
                get(handle,'device');
                if(a<=0||b<0)throw new RangeError('Invalid vertex buffer');
                return add({kind:'buffer',device,buffer:gl.createBuffer(),stride:a,count:b});
            }
            case 14: {
                get(handle,'device');
                const limit=gl.getParameter(gl.MAX_TEXTURE_SIZE);
                if(a<=0||b<=0||a>limit||b>limit||c<=0)throw new RangeError('Invalid texture size');
                const texture=gl.createTexture();gl.bindTexture(gl.TEXTURE_2D,texture);
                gl.texStorage2D(gl.TEXTURE_2D,c,gl.RGBA8,a,b);
                gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MAX_LEVEL,c-1);
                return add({kind:'texture',device,texture,width:a,height:b,levels:c});
            }
            case 5: {
                get(handle,'device');
                const limit=gl?.getParameter(gl.MAX_RENDERBUFFER_SIZE)??16384;
                if(!Number.isInteger(a)||!Number.isInteger(b)||a<=0||b<=0||a>limit||b>limit)throw new RangeError('Invalid GPU target size');
                const target={kind:'target',device,width:a,height:b};
                if(gl) {
                    target.framebuffer=gl.createFramebuffer();target.texture=gl.createTexture();target.depth=gl.createRenderbuffer();
                    gl.bindTexture(gl.TEXTURE_2D,target.texture);gl.texImage2D(gl.TEXTURE_2D,0,gl.RGBA8,a,b,0,gl.RGBA,gl.UNSIGNED_BYTE,null);
                    gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MIN_FILTER,gl.NEAREST);gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MAG_FILTER,gl.NEAREST);
                    gl.bindRenderbuffer(gl.RENDERBUFFER,target.depth);gl.renderbufferStorage(gl.RENDERBUFFER,gl.DEPTH_COMPONENT24,a,b);
                    gl.bindFramebuffer(gl.FRAMEBUFFER,target.framebuffer);gl.framebufferTexture2D(gl.FRAMEBUFFER,gl.COLOR_ATTACHMENT0,gl.TEXTURE_2D,target.texture,0);
                    gl.framebufferRenderbuffer(gl.FRAMEBUFFER,gl.DEPTH_ATTACHMENT,gl.RENDERBUFFER,target.depth);
                    if(gl.checkFramebufferStatus(gl.FRAMEBUFFER)!==gl.FRAMEBUFFER_COMPLETE){gl.deleteFramebuffer(target.framebuffer);gl.deleteTexture(target.texture);gl.deleteRenderbuffer(target.depth);throw new Error('Incomplete GPU target');}
                }
                return add(target);
            }
        }
        get(handle,'commands');
        switch(op) {
            case 6:
                if(r.inPass)throw new Error('GPU render pass already open');
                r.target=owned(a,'target');r.targetHandle=a;r.inPass=true;
                if(gl){gl.bindFramebuffer(gl.FRAMEBUFFER,r.target.framebuffer);gl.viewport(0,0,r.target.width,r.target.height);gl.disable(gl.SCISSOR_TEST);gl.bindVertexArray(r.vao);}
                break;
            case 7:
                if(!r.inPass)throw new Error('GPU clear outside render pass');
                if(gl){gl.colorMask(true,true,true,true);gl.depthMask(true);gl.clearColor(float(a),float(b),float(c),float(d));gl.clearDepth(1);gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);}
                break;
            case 8: {
                if(!r.inPass)throw new Error('GPU pipeline outside render pass');
                const vs=owned(a,'vertex'),fs=owned(b,'fragment'),key=`${a}:${b}`;
                let pipeline=device.programs.get(key);
                if(!pipeline) {
                    pipeline={vertex:a,fragment:b,bytes:0};
                    if(gl) {
                        const p=gl.createProgram();gl.attachShader(p,vs.shader);gl.attachShader(p,fs.shader);gl.linkProgram(p);
                        if(!gl.getProgramParameter(p,gl.LINK_STATUS)){const message=gl.getProgramInfoLog(p);gl.deleteProgram(p);throw new Error(message);}
                        pipeline.program=p;pipeline.textures=[];
                        for(let i=0;i<gl.getProgramParameter(p,gl.ACTIVE_UNIFORMS);i++){const u=gl.getActiveUniform(p,i);if(u.type===gl.SAMPLER_2D)pipeline.textures.push(gl.getUniformLocation(p,u.name));}
                        const block=gl.getUniformBlockIndex(p,'XuiBindings');
                        if(block!==gl.INVALID_INDEX){gl.uniformBlockBinding(p,block,0);pipeline.bytes=gl.getActiveUniformBlockParameter(p,block,gl.UNIFORM_BLOCK_DATA_SIZE);}
                    }
                    device.programs.set(key,pipeline);
                }
                if(d<0||d>2)throw new Error('Unsupported cull mode');
                r.pipeline=pipeline;r.attributes=[];
                if(gl){gl.useProgram(pipeline.program);(c&1?gl.enable:gl.disable).call(gl,gl.DEPTH_TEST);gl.depthFunc(gl.LESS);gl.depthMask(Boolean(c&2));
                    (c&4?gl.enable:gl.disable).call(gl,gl.BLEND);gl.blendFunc(gl.ONE,gl.ONE_MINUS_SRC_ALPHA);
                    (d?gl.enable:gl.disable).call(gl,gl.CULL_FACE);gl.frontFace(gl.CCW);if(d)gl.cullFace(d===1?gl.BACK:gl.FRONT);}
                break;
            }
            case 9:
                if(a!==r.attributes.length||b<1||b>4||a>=16)throw new Error('Invalid GPU vertex attribute');r.attributes.push(b);break;
            case 15:
                r.boundBuffer=owned(a,'buffer');r.stride=r.boundBuffer.stride;r.count=r.boundBuffer.count;break;
            case 16: {
                const texture=owned(b,'texture');
                if(!r.pipeline||a<0||a>=r.pipeline.textures.length)throw new Error('Texture slot is not declared by the pipeline');
                let sampler=r.samplers.get(a);if(!sampler){sampler=gl.createSampler();r.samplers.set(a,sampler);}
                const wrap=[gl.REPEAT,gl.CLAMP_TO_EDGE,gl.MIRRORED_REPEAT];
                gl.samplerParameteri(sampler,gl.TEXTURE_MIN_FILTER,(c&15)===0?gl.NEAREST_MIPMAP_NEAREST:gl.LINEAR_MIPMAP_LINEAR);
                gl.samplerParameteri(sampler,gl.TEXTURE_MAG_FILTER,(c>>4)===0?gl.NEAREST:gl.LINEAR);
                gl.samplerParameteri(sampler,gl.TEXTURE_WRAP_S,wrap[d&15]);gl.samplerParameteri(sampler,gl.TEXTURE_WRAP_T,wrap[d>>4]);
                gl.activeTexture(gl.TEXTURE0+a);gl.bindTexture(gl.TEXTURE_2D,texture.texture);gl.bindSampler(a,sampler);
                gl.uniform1i(r.pipeline.textures[a],a);break;
            }
            case 10: {
                if(!r.inPass||!r.pipeline||a<0||a>r.count||!r.attributes.length||r.attributes.reduce((sum,n)=>sum+n*4,0)>r.stride)throw new Error('Incomplete GPU draw');
                owned(r.targetHandle,'target');owned(r.pipeline.vertex,'vertex');owned(r.pipeline.fragment,'fragment');
                if((r.constantBytes??0)<r.pipeline.bytes)throw new Error('GPU uniform buffer is too short');
                if(gl){gl.useProgram(r.pipeline.program);gl.bindVertexArray(r.vao);gl.bindBuffer(gl.ARRAY_BUFFER,r.boundBuffer?.buffer??r.vertices);gl.bindBufferBase(gl.UNIFORM_BUFFER,0,r.constants);
                    let offset=0;for(let i=0;i<16;i++){if(i<r.attributes.length){gl.enableVertexAttribArray(i);gl.vertexAttribPointer(i,r.attributes[i],gl.FLOAT,false,r.stride,offset);offset+=r.attributes[i]*4;}else gl.disableVertexAttribArray(i);}
                    gl.drawArrays(gl.TRIANGLES,0,a);}
                draws++;break;
            }
            case 11:if(!r.inPass)throw new Error('No GPU render pass');r.inPass=false;break;
            case 12:if(r.inPass)throw new Error('End GPU render pass before commit');gl?.flush();break;
            default:throw new Error(`Unsupported GPU operation ${op}`);
        }
        return 0;
    }
    function upload(op,handle,view,a,b) {
        const r=get(handle),gl=r.device.gl;
        if(gl.isContextLost())throw new Error('WebGL context lost. Reload to recreate GPU resources.');
        if(scratch.length<view.length)scratch=new Uint8Array(Math.max(view.length,scratch.length*2));
        const data=scratch.subarray(0,view.length);
        if(op===4) {
            get(handle,'target');
            if(view.length!==r.width*r.height*4)throw new Error('Invalid readback length');
            gl.bindFramebuffer(gl.FRAMEBUFFER,r.framebuffer);gl.readPixels(0,0,r.width,r.height,gl.RGBA,gl.UNSIGNED_BYTE,data);
            const row=r.width*4;
            // Flip GL's bottom-up pixels and convert RGBA to the framework's BGRA.
            const output=new Uint8Array(data.length);
            for(let y=0;y<r.height;y++)for(let x=0;x<row;x+=4){const src=y*row+x,dst=(r.height-1-y)*row+x;output[dst]=data[src+2];output[dst+1]=data[src+1];output[dst+2]=data[src];output[dst+3]=data[src+3];}
            view.set(output);return;
        }
        view.copyTo(data);uploads++;
        switch(op) {
            case 0:
                get(handle,'commands');if(a<=0||b<0||data.length!==a*b)throw new RangeError('Invalid vertex upload');
                r.stride=a;r.count=b;r.boundBuffer=null;gl.bindBuffer(gl.ARRAY_BUFFER,r.vertices);gl.bufferData(gl.ARRAY_BUFFER,data,gl.DYNAMIC_DRAW);break;
            case 1:
                get(handle,'commands');r.constantBytes=data.length;gl.bindBuffer(gl.UNIFORM_BUFFER,r.constants);gl.bufferData(gl.UNIFORM_BUFFER,data,gl.DYNAMIC_DRAW);gl.bindBufferBase(gl.UNIFORM_BUFFER,0,r.constants);break;
            case 2:
                get(handle,'buffer');if(data.length!==r.stride*r.count)throw new RangeError('Invalid immutable upload');
                gl.bindBuffer(gl.ARRAY_BUFFER,r.buffer);gl.bufferData(gl.ARRAY_BUFFER,data,gl.STATIC_DRAW);break;
            case 3: {
                get(handle,'texture');if(a<0||a>=r.levels)throw new RangeError('Invalid mip level');
                const width=Math.max(1,r.width>>a),height=Math.max(1,r.height>>a);
                if(data.length!==width*height*4)throw new RangeError('Invalid mip data');
                gl.bindTexture(gl.TEXTURE_2D,r.texture);gl.pixelStorei(gl.UNPACK_ALIGNMENT,1);
                gl.texSubImage2D(gl.TEXTURE_2D,a,0,0,width,height,gl.RGBA,gl.UNSIGNED_BYTE,data);break;
            }
            default:throw new Error(`Unknown GPU upload ${op}`);
        }
    }
    return {
        invoke,upload,
        present(handle,context,x,y,width,height) {
            const target=get(handle,'target'),{gl,canvas}=target.device;
            if(!context||recordOnly)return;
            if(canvas.width!==target.width||canvas.height!==target.height){canvas.width=target.width;canvas.height=target.height;}
            gl.bindFramebuffer(gl.READ_FRAMEBUFFER,target.framebuffer);gl.bindFramebuffer(gl.DRAW_FRAMEBUFFER,null);
            gl.disable(gl.SCISSOR_TEST);gl.blitFramebuffer(0,0,target.width,target.height,0,0,target.width,target.height,gl.COLOR_BUFFER_BIT,gl.NEAREST);
            context.drawImage(canvas,x,y,width,height);
            if(context.canvas.dataset)context.canvas.dataset.xuiGpuSurface=String(handle);
        },
        get stats(){return {resources:resources.size,draws,uploads,renderer};},
        dispose(){for(const [id,r] of [...resources])if(r.kind==='device')release(id);}
    };
}
