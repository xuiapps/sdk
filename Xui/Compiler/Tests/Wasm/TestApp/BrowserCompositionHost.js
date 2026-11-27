// DOM surfaces for the compiler host. Managed keys stay opaque and are scoped to siblings.
// Content is redrawn each frame; keyed elements and their backing buffers are reused.
function createCompositionImports(canvas, commands) {
    const mount=document.createElement('div');
    mount.dataset.xuiComposition='';
    Object.assign(mount.style,{position:'fixed',left:'0',top:'0',pointerEvents:'none',zIndex:'1'});
    canvas.after(mount);
    const root={children:new Map(),element:mount,content:mount,canvas,placement:new DOMMatrix(),bounds:[0,0,0,0]};
    let current=root, frame=0, ratio=1, viewport=[0,0,0,0], pending, groups=[];
    const drawing=new Set(['FillRect','StrokeRect','FillText','DrawImage','GpuDraw','Fill','Stroke']);
    const rgba=([r,g,b,a],alpha=1)=>`rgba(${r*255},${g*255},${b*255},${a*alpha})`;
    const union=(a,b)=>!a ? b : !b ? a : [Math.min(a[0],b[0]),Math.min(a[1],b[1]),Math.max(a[2],b[2]),Math.max(a[3],b[3])];
    const intersect=(a,b)=>[Math.max(a[0],b[0]),Math.max(a[1],b[1]),Math.min(a[2],b[2]),Math.min(a[3],b[3])];
    function rect(matrix,x,y,w,h) {
        const p=[[x,y],[x+w,y],[x,y+h],[x+w,y+h]].map(([x,y])=>matrix.transformPoint({x,y}));
        return [Math.min(...p.map(v=>v.x)),Math.min(...p.map(v=>v.y)),Math.max(...p.map(v=>v.x)),Math.max(...p.map(v=>v.y))];
    }
    function prepare(node,state) {
        if(!node.probe){node.probe=document.createElement('canvas');node.probe.width=node.probe.height=1;node.ctx=node.probe.getContext('2d');}
        node.ctx.reset();node.commands=[];node.api=canvasImports(node.ctx,node.commands);
        if(state)node.api.SetState(state);
        node.initial=node.api.State();node.paint=null;node.path=null;node.clipped=false;node.saves=[];node.hasChildren=false;
    }
    function newNode() {
        const element=document.createElement('div'), effects=document.createElement('div'), content=document.createElement('div'), surface=document.createElement('canvas');
        element.dataset.xuiSurface='';effects.dataset.xuiEffects='';content.dataset.xuiContent='';
        for(const e of [element,effects,content,surface])Object.assign(e.style,{position:'absolute',left:'0',top:'0',transformOrigin:'0 0',pointerEvents:'none'});
        for(const e of [effects,content])Object.assign(e.style,{width:'100%',height:'100%'});
        content.append(surface);element.append(effects,content);
        return {children:new Map(),element,effects,content,canvas:surface};
    }
    function validate(o,key) {
        if(!key || typeof key!=='object')throw new Error('A group requires a stable object key');
        if((o.backing||o.clip||o.background||o.border||o.shadow||o.backdrop)&&!o.bounds)throw new Error('Group presentation requires bounds');
        if(!Number.isFinite(o.radius)||o.radius<0||!Number.isInteger(o.backdrop)||o.backdrop<0||o.backdrop>2)throw new Error('Invalid group outline/material');
        if(!o.matrix.every(Number.isFinite)||o.bounds&&(!o.bounds.every(Number.isFinite)||o.bounds[2]<0||o.bounds[3]<0))throw new Error('Invalid group geometry');
        if(o.border&&(!Number.isFinite(o.border[0])||o.border[0]<0)||o.shadow&&(!o.shadow.every(Number.isFinite)||o.shadow[0]<0))throw new Error('Invalid group border/shadow');
    }
    function group(key) {
        const o=pending;pending=null;validate(o,key);
        if(!o.backing||current.clipped){groups.push({parent:current,native:false,depth:current.saves.length});return 0;}
        const parent=current;
        let node=parent.children.get(key);
        if(!node){node=newNode();parent.children.set(key,node);}
        if(node.seen===frame)throw new Error('Duplicate sibling composition key');
        node.seen=frame;node.options=o;node.bounds=o.bounds;
        const local=parent.ctx.getTransform().multiply(new DOMMatrix(o.matrix));
        node.placement=parent.placement.multiply(local);
        const relative=new DOMMatrix().translate(-parent.bounds[0],-parent.bounds[1]).multiply(local).translate(o.bounds[0],o.bounds[1]);
        Object.assign(node.element.style,{width:`${o.bounds[2]}px`,height:`${o.bounds[3]}px`,transform:relative.toString()});
        const state=parent.api.State();
        prepare(node,state);
        const alpha=state.paint.globalAlpha;
        Object.assign(node.effects.style,{borderRadius:`${o.radius}px`,boxSizing:'border-box',
            opacity:alpha,background:o.background?rgba(o.background):o.backdrop?'rgba(245,248,255,.55)':'transparent',
            border:o.border?`${Math.min(o.border[0],o.bounds[2],o.bounds[3])}px solid ${rgba(o.borderColor)}`:'none',
            boxShadow:o.shadow?`${o.shadow[1]}px ${o.shadow[2]}px ${o.shadow[0]}px ${rgba(o.shadowColor)}`:'none',
            backdropFilter:o.backdrop?'blur(18px) saturate(1.3)':'none',webkitBackdropFilter:o.backdrop?'blur(18px) saturate(1.3)':'none'});
        Object.assign(node.content.style,{overflow:o.clip?'hidden':'visible',borderRadius:o.clip?`${o.radius}px`:'0'});
        parent.content.append(node.element);parent.hasChildren=true;
        groups.push({parent,native:true});current=node;
        return 1;
    }
    function endGroup() {
        const group=groups.pop();if(!group)throw new Error('No composition group to close');
        if(group.native){if(current.saves.length)throw new Error('Unbalanced saves inside composition group');current=group.parent;return 1;}
        if(current.saves.length!==group.depth+1)throw new Error('Unbalanced saves inside immediate group');
        return 0; // The C# immediate fallback performs its paired Restore next.
    }
    function command(name,a,b,c,d,text) {
        commands.push([name,a,b,c,d,text]);
        if(name.startsWith('Group')){
            if(name==='GroupReset')pending={backing:!!a,clip:!!b,radius:c,backdrop:d,matrix:[1,0,0,1,0,0]};
            else if(name==='GroupMatrix')pending.matrix.splice(0,4,a,b,c,d);
            else if(name==='GroupTranslation')pending.matrix.splice(4,2,a,b);
            else if(name==='GroupBounds')pending.bounds=[a,b,c,d];
            else if(name==='GroupBackground')pending.background=[a,b,c,d];
            else if(name==='GroupBorderColor')pending.borderColor=[a,b,c,d];
            else if(name==='GroupBorder')pending.border=[a];
            else if(name==='GroupShadowColor')pending.shadowColor=[a,b,c,d];
            else if(name==='GroupShadow')pending.shadow=[a,b,c];
            else throw new Error(`Unknown group property: ${name}`);
            return;
        }
        const n=current,m=n.ctx.getTransform(),point=(x,y)=>rect(m,x,y,0,0);
        if(name==='Save')n.saves.push(n.clipped);
        if(name==='Restore'){if(!n.saves.length)throw new Error('Unbalanced Restore');n.clipped=n.saves.pop();}
        if(name==='Clip')n.clipped=true;
        if(name==='BeginPath')n.path=null;
        if(name==='MoveTo'||name==='LineTo')n.path=union(n.path,point(a,b));
        if(name==='QuadraticTo'||name==='CurveControls')n.path=union(n.path,union(point(a,b),point(c,d)));
        if(name==='CubicTo')n.path=union(n.path,point(a,b));
        if(name==='Rect')n.path=union(n.path,rect(m,a,b,c,d));
        if(name==='PathRect')n.pathRect=[a,b,c,d];
        if(name==='RoundRect'||name==='RoundRectCorners')n.path=union(n.path,rect(m,...n.pathRect));
        if(name==='ArcOrigin')n.path=union(n.path,rect(m,a-c,b-c,c*2,c*2));
        if(name==='EllipseGeometry'){const r=Math.max(c,d);n.path=union(n.path,rect(m,a-r,b-r,2*r,2*r));}
        // ArcTo tangent distances can exceed its control points by an arbitrary
        // amount. Conservatively include the visible viewport for those paths.
        if(name==='ArcTo')n.path=union(n.path,rect(n.placement.inverse(),...viewport));
        if(drawing.has(name)){
            if(n.hasChildren)throw new Error('Draw background content before child surfaces; use an overlay group for foreground drawing');
            let bounds=n.path;
            if(name==='FillRect'||name==='StrokeRect'||name==='DrawImage'||name==='GpuDraw')bounds=rect(m,a,b,c,d);
            if(name==='FillText'){
                const q=n.ctx.measureText(text);
                bounds=rect(m,a-q.actualBoundingBoxLeft,b-q.actualBoundingBoxAscent,q.actualBoundingBoxLeft+q.actualBoundingBoxRight,q.actualBoundingBoxAscent+q.actualBoundingBoxDescent);
            }
            if(bounds){
                const pad=name==='Stroke'||name==='StrokeRect'?n.ctx.lineWidth*Math.max(1,n.ctx.miterLimit)*(Math.hypot(m.a,m.b)+Math.hypot(m.c,m.d)):1;
                n.paint=union(n.paint,[bounds[0]-pad,bounds[1]-pad,bounds[2]+pad,bounds[3]+pad]);
            }
        }
        n.api.Command(name,a,b,c,d,text);
    }
    function render(node) {
        if(node!==root){
            let area=node.paint;
            if(area&&node.options.clip)area=intersect(area,[node.bounds[0],node.bounds[1],node.bounds[0]+node.bounds[2],node.bounds[1]+node.bounds[3]]);
            if(area)area=intersect(area,rect(node.placement.inverse(),...viewport));
            const scale=ratio*Math.max(Math.hypot(node.placement.a,node.placement.b),Math.hypot(node.placement.c,node.placement.d));
            const visible=area&&scale>0&&area[2]>area[0]&&area[3]>area[1]&&area.every(Number.isFinite);
            const x=visible?Math.floor(area[0]*scale)/scale:0,y=visible?Math.floor(area[1]*scale)/scale:0;
            const width=visible?Math.ceil((area[2]-x)*scale):1,height=visible?Math.ceil((area[3]-y)*scale):1;
            if(node.canvas.width!==width)node.canvas.width=width;
            if(node.canvas.height!==height)node.canvas.height=height;
            Object.assign(node.canvas.style,{display:visible?'block':'none',left:`${x-node.bounds[0]}px`,top:`${y-node.bounds[1]}px`,width:`${visible?width/scale:0}px`,height:`${visible?height/scale:0}px`});
            const ctx=node.canvas.getContext('2d');ctx.reset();
            if(visible){const base=new DOMMatrix().scale(scale).translate(-x,-y);ctx.setTransform(base);const api=canvasImports(ctx,[],base);api.SetState(node.initial);for(const c of node.commands)api.Command(...c);}
        }else{
            const ctx=canvas.getContext('2d'),base=new DOMMatrix().scale(ratio);ctx.setTransform(base);
            const api=canvasImports(ctx,[],base);api.SetState(node.initial);for(const c of node.commands)api.Command(...c);
        }
        for(const [key,child] of node.children){if(child.seen!==frame){child.element.remove();node.children.delete(key);}else render(child);}
    }
    prepare(root);
    return {Command:command,Group:group,EndGroup:endGroup,
        Measure:(...args)=>current.api.Measure(...args),LoadImage:(...args)=>current.api.LoadImage(...args),ImageSize:(...args)=>current.api.ImageSize(...args),
        beginFrame(width,height,dpr){frame++;ratio=dpr;viewport=[0,0,width,height];current=root;groups=[];prepare(root);Object.assign(mount.style,{width:`${width}px`,height:`${height}px`});},
        endFrame(){if(groups.length||current!==root||root.saves.length)throw new Error('Unbalanced composition frame');render(root);},
        dispose(){mount.remove();root.children.clear();groups=[];}
    };
}
