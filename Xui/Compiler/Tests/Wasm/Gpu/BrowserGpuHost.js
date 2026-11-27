// The host consumes WASM upload memory synchronously; no typed view survives Invoke.
function createGpuHost(memory, {recordOnly = false} = {}) {
    const resources = new Map();let next=1,draws=0,uploads=0;
    const floatBits=new DataView(new ArrayBuffer(4));
    const float=value=>{floatBits.setInt32(0,value,true);return floatBits.getFloat32(0,true);};
    const add=value=>{const handle=next++;resources.set(handle,value);return handle;};
    const get=(handle,kind)=>{const value=resources.get(handle);if(!value || kind&&value.kind!==kind)throw new Error(`Invalid GPU ${kind??'resource'} handle ${handle}`);return value;};
    const bytes=(pointer,length)=>{
        const buffer=memory().buffer;
        if(!Number.isInteger(pointer)||pointer<0||!Number.isInteger(length)||length<0||pointer>buffer.byteLength-length)throw new RangeError('GPU upload exceeds WASM memory');
        return new Uint8Array(buffer,pointer,length);
    };
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
        else if(r.kind==='commands') {gl?.deleteBuffer(r.vertices);gl?.deleteBuffer(r.constants);gl?.deleteVertexArray(r.vao);}
        else if(r.kind==='vertex'||r.kind==='fragment') {
            for(const [key,p] of r.device.programs)if(p.vertex===handle||p.fragment===handle){gl?.deleteProgram(p.program);r.device.programs.delete(key);}
            gl?.deleteShader(r.shader);
        }
        resources.delete(handle);
    }
    function invoke(op,handle,a,b,c,d,text) {
        if(op==='Scale')return Math.ceil((globalThis.devicePixelRatio??1)*1024);
        if(op==='Device') {
            let canvas=null,gl=null;
            if(!recordOnly){canvas=new OffscreenCanvas(1,1);gl=canvas.getContext('webgl2',{alpha:true,antialias:false,preserveDrawingBuffer:true});if(!gl)throw new Error('WebGL 2 is unavailable');}
            return add({kind:'device',canvas,gl,programs:new Map()});
        }
        if(op==='Dispose'){release(handle);return 0;}
        const r=get(handle),device=r.kind==='device'?r:r.device,gl=device.gl;
        const owned=(id,kind)=>{const value=get(id,kind);if(value.device!==device)throw new Error('GPU resource belongs to another device');return value;};
        switch(op) {
            case 'VertexShader':case 'FragmentShader':
                get(handle,'device');return add({kind:op==='VertexShader'?'vertex':'fragment',device,shader:compile(device,text,op==='VertexShader')});
            case 'Commands':
                get(handle,'device');return add({kind:'commands',device,vao:gl?.createVertexArray(),vertices:gl?.createBuffer(),constants:gl?.createBuffer(),attributes:[],inPass:false});
            case 'Target': {
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
            case 'Begin':
                if(r.inPass)throw new Error('GPU render pass already open');
                r.target=owned(a,'target');r.targetHandle=a;r.inPass=true;
                if(gl){gl.bindFramebuffer(gl.FRAMEBUFFER,r.target.framebuffer);gl.viewport(0,0,r.target.width,r.target.height);gl.disable(gl.SCISSOR_TEST);gl.bindVertexArray(r.vao);}
                break;
            case 'Clear':
                if(!r.inPass)throw new Error('GPU clear outside render pass');
                if(gl){gl.colorMask(true,true,true,true);gl.depthMask(true);gl.clearColor(float(a),float(b),float(c),float(d));gl.clearDepth(1);gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);}
                break;
            case 'Pipeline': {
                if(!r.inPass)throw new Error('GPU pipeline outside render pass');
                const vs=owned(a,'vertex'),fs=owned(b,'fragment'),key=`${a}:${b}`;
                let pipeline=device.programs.get(key);
                if(!pipeline) {
                    pipeline={vertex:a,fragment:b,bytes:0};
                    if(gl) {
                        const p=gl.createProgram();gl.attachShader(p,vs.shader);gl.attachShader(p,fs.shader);gl.linkProgram(p);
                        if(!gl.getProgramParameter(p,gl.LINK_STATUS)){const message=gl.getProgramInfoLog(p);gl.deleteProgram(p);throw new Error(message);}
                        pipeline.program=p;const block=gl.getUniformBlockIndex(p,'XuiBindings');
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
            case 'Attribute':
                if(a!==r.attributes.length||b<1||b>4||a>=16)throw new Error('Invalid GPU vertex attribute');r.attributes.push(b);break;
            case 'Vertices': {
                if(c<=0||d<0||b!==c*d)throw new RangeError('Invalid GPU vertex upload');
                const data=bytes(a,b);r.stride=c;r.count=d;uploads++;
                if(gl){gl.bindBuffer(gl.ARRAY_BUFFER,r.vertices);gl.bufferData(gl.ARRAY_BUFFER,data,gl.DYNAMIC_DRAW);}break;
            }
            case 'Constants': {
                const data=bytes(a,b);r.constantBytes=b;uploads++;
                if(gl){gl.bindBuffer(gl.UNIFORM_BUFFER,r.constants);gl.bufferData(gl.UNIFORM_BUFFER,data,gl.DYNAMIC_DRAW);gl.bindBufferBase(gl.UNIFORM_BUFFER,0,r.constants);}break;
            }
            case 'Draw': {
                if(!r.inPass||!r.pipeline||a<0||a>r.count||!r.attributes.length||r.attributes.reduce((sum,n)=>sum+n*4,0)>r.stride)throw new Error('Incomplete GPU draw');
                owned(r.targetHandle,'target');owned(r.pipeline.vertex,'vertex');owned(r.pipeline.fragment,'fragment');
                if((r.constantBytes??0)<r.pipeline.bytes)throw new Error('GPU uniform buffer is too short');
                if(gl){gl.useProgram(r.pipeline.program);gl.bindVertexArray(r.vao);gl.bindBuffer(gl.ARRAY_BUFFER,r.vertices);gl.bindBufferBase(gl.UNIFORM_BUFFER,0,r.constants);
                    let offset=0;for(let i=0;i<16;i++){if(i<r.attributes.length){gl.enableVertexAttribArray(i);gl.vertexAttribPointer(i,r.attributes[i],gl.FLOAT,false,r.stride,offset);offset+=r.attributes[i]*4;}else gl.disableVertexAttribArray(i);}
                    gl.drawArrays(gl.TRIANGLES,0,a);const error=gl.getError();if(error!==gl.NO_ERROR)throw new Error(`WebGL draw error ${error}`);}
                draws++;break;
            }
            case 'End':if(!r.inPass)throw new Error('No GPU render pass');r.inPass=false;break;
            case 'Commit':if(r.inPass)throw new Error('End GPU render pass before commit');gl?.flush();break;
            default:throw new Error(`Unsupported GPU operation ${op}`);
        }
        return 0;
    }
    return {
        imports:{Invoke:invoke},
        present(handle,context,x,y,width,height) {
            const target=get(handle,'target'),{gl,canvas}=target.device;
            if(!context||recordOnly)return;
            if(canvas.width!==target.width||canvas.height!==target.height){canvas.width=target.width;canvas.height=target.height;}
            gl.bindFramebuffer(gl.READ_FRAMEBUFFER,target.framebuffer);gl.bindFramebuffer(gl.DRAW_FRAMEBUFFER,null);
            gl.disable(gl.SCISSOR_TEST);gl.blitFramebuffer(0,0,target.width,target.height,0,0,target.width,target.height,gl.COLOR_BUFFER_BIT,gl.NEAREST);
            context.drawImage(canvas,x,y,width,height);
            if(context.canvas.dataset)context.canvas.dataset.xuiGpuSurface=String(handle);
        },
        get stats(){return {resources:resources.size,draws,uploads};},
        dispose(){for(const [id,r] of [...resources])if(r.kind==='device')release(id);}
    };
}
