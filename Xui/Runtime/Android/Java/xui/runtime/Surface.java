package xui.runtime;

import android.content.Context;
import android.graphics.*;
import android.view.*;
import java.nio.*;

/** One explicitly backed Xui group; Android owns its retained hardware display list. */
final class Surface extends ViewGroup {
    final int id;
    final CanvasPlayer player;
    private final Host host;
    long seen;
    long gpuFrameCount;
    private final TextureView gpu;
    private ByteBuffer commands=ByteBuffer.allocateDirect(0).order(ByteOrder.LITTLE_ENDIAN);
    private final Matrix transform=new Matrix();
    private final float[] matrix=new float[9], previousMatrix=new float[9];
    private final Paint decoration=new Paint(Paint.ANTI_ALIAS_FLAG);
    private final RectF rect=new RectF();
    private float originX,originY,scale=1,radius,borderWidth;
    private int background,border,backdrop;
    Surface(Context context,Host host,int id) { this(context,host,id,false); }
    Surface(Context context,Host host,int id,boolean isGpu) {
        super(context);this.id=id;this.host=host;player=new CanvasPlayer(host);
        gpu=isGpu?new TextureView(context):null;
        if(gpu!=null) {
            gpu.setOpaque(true);
            gpu.setSurfaceTextureListener(new TextureView.SurfaceTextureListener() {
                private android.view.Surface surface;
                public void onSurfaceTextureAvailable(SurfaceTexture texture,int w,int h) {surface=new android.view.Surface(texture);host.gpuSurface(id,surface,w,h);}
                public void onSurfaceTextureSizeChanged(SurfaceTexture texture,int w,int h) {host.gpuSurface(id,surface,w,h);}
                public boolean onSurfaceTextureDestroyed(SurfaceTexture texture) {host.gpuSurface(id,null,0,0);surface.release();surface=null;return true;}
                public void onSurfaceTextureUpdated(SurfaceTexture texture) {gpuFrameCount++;}
            });
            addView(gpu);
        }
        setWillNotDraw(false);setClipChildren(false);setClipToPadding(false);
        setImportantForAccessibility(IMPORTANT_FOR_ACCESSIBILITY_NO);
        setOutlineProvider(new ViewOutlineProvider() {
            @Override public void getOutline(View view,Outline outline) {
                outline.setRoundRect(0,0,getWidth(),getHeight(),radius*scale);
            }
        });
    }
    void update(ByteBuffer input) {
        float x=input.getFloat(),y=input.getFloat(),width=input.getFloat(),height=input.getFloat();
        matrix[0]=input.getFloat();matrix[3]=input.getFloat();matrix[1]=input.getFloat();matrix[4]=input.getFloat();
        float tx=input.getFloat(),ty=input.getFloat(),density=input.getFloat();
        matrix[2]=tx*density;matrix[5]=ty*density;matrix[6]=matrix[7]=0;matrix[8]=1;
        boolean clip=input.getInt()!=0;float corners=input.getFloat();int bg=CanvasPlayer.color(input,1),material=input.getInt();
        int stroke=CanvasPlayer.color(input,1);float strokeWidth=input.getFloat();
        int shadow=CanvasPlayer.color(input,1);float blur=input.getFloat();float shadowX=input.getFloat(),shadowY=input.getFloat();
        boolean repaint=x!=originX||y!=originY||density!=scale||corners!=radius||bg!=background||material!=backdrop||stroke!=border||strokeWidth!=borderWidth;
        originX=x;originY=y;scale=density;radius=corners;background=bg;border=stroke;borderWidth=strokeWidth;backdrop=material;
        int w=Math.max(0,(int)Math.ceil(width*density)),h=Math.max(0,(int)Math.ceil(height*density));
        if(w!=getWidth()||h!=getHeight()) {layout(0,0,w,h);repaint=true;}
        boolean moved=false;for(int i=0;i<9;i++)if(matrix[i]!=previousMatrix[i])moved=true;
        if(moved) {transform.setValues(matrix);setAnimationMatrix(transform);System.arraycopy(matrix,0,previousMatrix,0,9);}
        if(getClipToOutline()!=clip)setClipToOutline(clip);
        if(repaint)invalidateOutline();
        // Android elevation supplies a native outline shadow. Radius/offset are approximations,
        // not CSS shadow parameters; backdrop material is a translucent portable approximation.
        float elevation=Color.alpha(shadow)==0?0:blur*density/2;
        if(getElevation()!=elevation)setElevation(elevation);
        int nativeShadow=host.shadowColor(shadow);
        if(getOutlineAmbientShadowColor()!=nativeShadow)setOutlineAmbientShadowColor(nativeShadow);
        if(getOutlineSpotShadowColor()!=nativeShadow)setOutlineSpotShadowColor(nativeShadow);
        int bytes=input.getInt();
        if(bytes>=0) {
            if(bytes>input.remaining())throw new IllegalArgumentException("Truncated drawing buffer");
            boolean changed=bytes!=commands.limit();
            int start=input.position();
            if(!changed)for(int i=0;i<bytes;i++)if(commands.get(i)!=input.get(start+i)){changed=true;break;}
            if(changed) {
                if(commands.capacity()<bytes)commands=ByteBuffer.allocateDirect(Math.max(bytes,Math.max(256,commands.capacity()*2))).order(ByteOrder.LITTLE_ENDIAN);
                commands.clear();for(int i=0;i<bytes;i++)commands.put(input.get());commands.flip();repaint=true;
            } else input.position(start+bytes);
        } else if(bytes!=-1&&!(bytes==-2&&gpu!=null))throw new IllegalArgumentException("Invalid content length");
        if(repaint)invalidate();
    }
    @Override protected void onLayout(boolean changed,int l,int t,int r,int b) { if(gpu!=null)gpu.layout(0,0,r-l,b-t); /* Other children are placed by Xui. */ }
    @Override protected void dispatchDraw(Canvas canvas) {
        // Each child is a separate Z block: native elevation draws its shadow but
        // cannot reorder siblings across Xui's explicitly declared paint order.
        for (int i=0;i<getChildCount();i++) {
            canvas.enableZ();
            try { drawChild(canvas,getChildAt(i),getDrawingTime()); }
            finally { canvas.disableZ(); }
        }
    }
    @Override protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        float inset=borderWidth*scale/2;
        if(backdrop!=0) {decoration.setStyle(Paint.Style.FILL);decoration.setColor(0xddeaf1fa);rect.set(0,0,getWidth(),getHeight());canvas.drawRoundRect(rect,radius*scale,radius*scale,decoration);}
        if(Color.alpha(background)!=0) {decoration.setStyle(Paint.Style.FILL);decoration.setColor(background);rect.set(0,0,getWidth(),getHeight());canvas.drawRoundRect(rect,radius*scale,radius*scale,decoration);}
        int save=canvas.save();canvas.scale(scale,scale);canvas.translate(-originX,-originY);
        try {player.replay(canvas,commands);} finally {canvas.restoreToCount(save);}
        if(borderWidth>0) {decoration.setStyle(Paint.Style.STROKE);decoration.setStrokeWidth(borderWidth*scale);decoration.setColor(border);rect.set(inset,inset,getWidth()-inset,getHeight()-inset);float r=Math.max(0,radius*scale-inset);canvas.drawRoundRect(rect,r,r,decoration);}
    }
}
