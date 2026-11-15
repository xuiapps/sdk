package xui.runtime;

import android.app.Activity;
import android.content.res.Configuration;
import android.animation.ValueAnimator;
import android.content.Context;
import android.graphics.*;
import android.os.*;
import android.util.SparseArray;
import android.view.*;
import android.view.inputmethod.*;
import java.io.*;
import java.nio.*;

/** Java owns all Android graphics objects. C# talks only to the C ABI. */
public final class Host extends ViewGroup implements Choreographer.FrameCallback {
    private long nativeHost;
    private final float shadowAlphaScale;
    int shadowColor(int color) {
        int alpha=Math.min(255,Math.round(Color.alpha(color)*shadowAlphaScale));
        return (color&0x00ffffff)|(alpha<<24);
    }
    private final TextInput input = new TextInput(this);
    void textEvent(ByteBuffer packet,int size) {if(nativeHost!=0)event(nativeHost,4,0,packet,size);}
    @Override public boolean onKeyDown(int code,KeyEvent event){return input.key(event)||super.onKeyDown(code,event);}
    @Override public boolean onCheckIsTextEditor(){return input.active();}
    @Override public InputConnection onCreateInputConnection(EditorInfo info){return input.connect(info);}
    private int statusBarAppearance;
    private void applyStatusBarAppearance() {
        boolean dark=statusBarAppearance==2 || (statusBarAppearance==0 &&
            (getResources().getConfiguration().uiMode & Configuration.UI_MODE_NIGHT_MASK)!=Configuration.UI_MODE_NIGHT_YES);
        Window window=((Activity)getContext()).getWindow();
        if(Build.VERSION.SDK_INT>=30) {
            WindowInsetsController controller=window.getInsetsController();
            if(controller!=null)controller.setSystemBarsAppearance(dark?WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS:0,
                WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS);
        } else {
            View decor=window.getDecorView();
            int flags=decor.getSystemUiVisibility();
            decor.setSystemUiVisibility(dark?flags|View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR:flags&~View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR);
        }
    }
    @Override protected void onAttachedToWindow() {super.onAttachedToWindow();applyStatusBarAppearance();}
    @Override protected void onConfigurationChanged(Configuration config) {super.onConfigurationChanged(config);applyStatusBarAppearance();}
    @Override public void onWindowFocusChanged(boolean focused) {super.onWindowFocusChanged(focused);if(focused)applyStatusBarAppearance();}
    private boolean active;
    private long frame;
    private final SparseArray<Surface> nodes=new SparseArray<>();
    private final SparseArray<Typeface> fonts=new SparseArray<>();
    private final SparseArray<Bitmap> images=new SparseArray<>();
    private final ByteBuffer events=ByteBuffer.allocateDirect(32*20).order(ByteOrder.LITTLE_ENDIAN);
    private final Paint measure=new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint.FontMetrics metrics=new Paint.FontMetrics();
    private final Rect glyphBounds=new Rect();
    private char[] text=new char[64];
    private float left,top,right,bottom;
    private static native void surface(long host,int id,android.view.Surface surface,int width,int height);
    void gpuSurface(int id,android.view.Surface surface,int width,int height) {if(nativeHost!=0)surface(nativeHost,id,surface,width,height);}
    private static native void event(long host,int kind,long time,ByteBuffer data,int length);

    public Host(Activity activity,long host) {
        super(activity);nativeHost=host;
        // Xui specifies the final shadow opacity. Android additionally multiplies
        // both shadow components by theme alpha; normalize that extra attenuation.
        android.content.res.TypedArray shadowTheme=activity.obtainStyledAttributes(
            new int[]{android.R.attr.ambientShadowAlpha,android.R.attr.spotShadowAlpha});
        try {
            float alpha=shadowTheme.getFloat(0,.039f)+shadowTheme.getFloat(1,.19f);
            shadowAlphaScale=alpha>0?1/alpha:1;
        } finally {shadowTheme.recycle();}
        if(Build.VERSION.SDK_INT<29)throw new UnsupportedOperationException("Xui composition requires Android 10 (API 29)");
        // Xui draws behind the status bar and handles the reported safe-area insets.
        // Do not let icon appearance introduce a system contrast scrim or theme color.
        Window window=activity.getWindow();
        window.clearFlags(WindowManager.LayoutParams.FLAG_TRANSLUCENT_STATUS);
        window.addFlags(WindowManager.LayoutParams.FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS);
        window.setStatusBarColor(Color.TRANSPARENT);
        window.setStatusBarContrastEnforced(false);
        if(Build.VERSION.SDK_INT>=30) window.setDecorFitsSystemWindows(false);
        else {
            View decor=window.getDecorView();
            decor.setSystemUiVisibility(decor.getSystemUiVisibility() |
                View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN | View.SYSTEM_UI_FLAG_LAYOUT_STABLE);
        }
        setClipChildren(false);setClipToPadding(false);setFocusableInTouchMode(true);
        setOnApplyWindowInsetsListener((v,insets)->{
            if(Build.VERSION.SDK_INT>=30) {
                Insets safe=insets.getInsets(WindowInsets.Type.systemBars()|WindowInsets.Type.displayCutout());
                left=safe.left;top=safe.top;right=safe.right;bottom=safe.bottom;
            } else {left=insets.getSystemWindowInsetLeft();top=insets.getSystemWindowInsetTop();right=insets.getSystemWindowInsetRight();bottom=insets.getSystemWindowInsetBottom();}
            return insets;
        });
        activity.setContentView(this);
    }
    @Override protected void onLayout(boolean changed,int l,int t,int r,int b) { }
    @Override protected void onMeasure(int width,int height) {setMeasuredDimension(MeasureSpec.getSize(width),MeasureSpec.getSize(height));}
    void active(boolean value) {
        if(active==value)return;active=value;
        Choreographer.getInstance().removeFrameCallback(this);
        if(active)Choreographer.getInstance().postFrameCallback(this);
    }
    public void dispose() {
        active(false);nativeHost=0;removeAllViews();nodes.clear();fonts.clear();
        // Do not recycle bitmaps still referenced by a RenderThread display list.
        images.clear();
    }
    @Override public void doFrame(long time) {
        if(!active||nativeHost==0)return;
        events.clear();events.putFloat(getWidth()).putFloat(getHeight()).putFloat(getResources().getDisplayMetrics().density)
            .putFloat(getDisplay()==null?60:getDisplay().getRefreshRate()).putFloat(left).putFloat(top).putFloat(right).putFloat(bottom);
        Configuration config=getResources().getConfiguration();
        events.putFloat(config.fontScale).putFloat(config.smallestScreenWidthDp>=600?1:0)
            .putFloat((config.uiMode & Configuration.UI_MODE_NIGHT_MASK)==Configuration.UI_MODE_NIGHT_YES?1:0)
            .putFloat(ValueAnimator.areAnimatorsEnabled()?0:1);
        event(nativeHost,1,time,events,events.position());
        if(active)Choreographer.getInstance().postFrameCallback(this);
    }
    @Override public boolean onInterceptTouchEvent(MotionEvent e) {return true;}
    @Override public boolean onTouchEvent(MotionEvent e) {
        if(nativeHost==0)return false;
        int count=e.getPointerCount();if(count>32)throw new IllegalArgumentException("Too many pointers");
        int action=e.getActionMasked(),changed=e.getActionIndex();float density=getResources().getDisplayMetrics().density;
        events.clear();
        for(int i=0;i<count;i++) {
            int phase=1;
            if(action==MotionEvent.ACTION_CANCEL)phase=3;
            else if(i==changed&&(action==MotionEvent.ACTION_DOWN||action==MotionEvent.ACTION_POINTER_DOWN))phase=0;
            else if(i==changed&&(action==MotionEvent.ACTION_UP||action==MotionEvent.ACTION_POINTER_UP))phase=2;
            events.putInt(e.getPointerId(i)).putInt(phase).putFloat(e.getX(i)/density).putFloat(e.getY(i)/density).putFloat(e.getTouchMajor(i)/density/2);
        }
        event(nativeHost,2,e.getEventTime()*1000000L,events,events.position());return true;
    }
    Typeface font(int id) {Typeface font=fonts.get(id);if(font==null)throw new IllegalArgumentException("Unknown font "+id);return font;}
    Bitmap image(int id) {Bitmap image=images.get(id);if(image==null)throw new IllegalArgumentException("Unknown image "+id);return image;}
    static String string(ByteBuffer b) {
        int count=b.getInt();if(count<0||count>b.remaining()/2)throw new IllegalArgumentException("Invalid string");
        char[] chars=new char[count];for(int i=0;i<count;i++)chars[i]=b.getChar();return new String(chars);
    }
    private void apply(ByteBuffer b) {
        if(b.getInt()!=0x58554931||b.getInt()!=1)throw new IllegalArgumentException("Unsupported Xui composition protocol");
        frame++;
        int removed=b.getInt();if(removed<0||removed>b.remaining()/4)throw new IllegalArgumentException("Invalid removal count");
        for(int i=0;i<removed;i++) {int id=b.getInt();Surface node=nodes.get(id);if(node!=null){if(node.getParent()!=null)((ViewGroup)node.getParent()).removeView(node);nodes.remove(id);}}
        int count=b.getInt();if(count<0||count>b.remaining()/12)throw new IllegalArgumentException("Invalid node count");
        for(int i=0;i<count;i++) {
            int id=b.getInt(),parentId=b.getInt(),index=b.getInt();
            Surface node=nodes.get(id);if(node==null){node=new Surface(getContext(),this,id,b.getInt(b.position()+120)==-2);nodes.put(id,node);}
            ViewGroup parent=parentId==0?this:nodes.get(parentId);
            if(parent==null||parent==node||node.seen==frame)throw new IllegalArgumentException("Invalid composition hierarchy");
            node.seen=frame;node.update(b);
            if(node.getParent()!=parent) {if(node.getParent()!=null)((ViewGroup)node.getParent()).removeView(node);parent.addView(node,index);}
            else if(parent.indexOfChild(node)!=index) {parent.removeView(node);parent.addView(node,index);}
        }
        // Culled nodes stay alive until the bounded C# cache evicts them.
        for(int i=0;i<nodes.size();i++) {Surface node=nodes.valueAt(i);if(node.seen!=frame&&node.getParent()!=null)((ViewGroup)node.getParent()).removeView(node);}
        if(b.hasRemaining())throw new IllegalArgumentException("Trailing composition data");
    }
    public void invoke(int operation,ByteBuffer b,int length) throws IOException {
        if(Looper.myLooper()!=Looper.getMainLooper())throw new IllegalStateException("Xui Android calls require the UI thread");
        b.order(ByteOrder.LITTLE_ENDIAN);b.clear();b.limit(length);
        switch(operation) {
            case 1:apply(b);break;
            case 2:{
                int id=b.getInt();String family=string(b);int weight=b.getInt();boolean italic=b.getInt()!=0;int bytes=b.getInt();
                if(bytes<0||bytes>b.remaining())throw new IllegalArgumentException("Invalid font data");
                Typeface face;
                if(bytes!=0) {
                    // Typeface.Builder accepts a file descriptor. Only first use of a face writes
                    // bytes; the temporary file is unlinked immediately after synchronous loading.
                    File file=File.createTempFile("xui-font-",".ttf",getContext().getCacheDir());
                    try {try(FileOutputStream out=new FileOutputStream(file)){ByteBuffer data=b.slice();data.limit(bytes);while(data.hasRemaining())out.getChannel().write(data);}
                        try(FileInputStream in=new FileInputStream(file)){face=new Typeface.Builder(in.getFD()).setWeight(weight).setItalic(italic).build();}
                    } finally {file.delete();}
                } else face=Typeface.create(Typeface.create(family,Typeface.NORMAL),weight,italic);
                fonts.put(id,face);break;
            }
            case 3:{int id=b.getInt();String path=string(b);Bitmap image;try(InputStream in=getContext().getAssets().open(path)){image=BitmapFactory.decodeStream(in);}if(image==null)throw new IOException("Cannot decode image: "+path);images.put(id,image);b.clear();b.putInt(image.getWidth()).putInt(image.getHeight());break;}
            case 4:{int id=b.getInt(),w=b.getInt(),h=b.getInt();if(w<=0||h<=0||(long)w*h*4!=b.remaining())throw new IllegalArgumentException("Invalid pixel data");int[] pixels=new int[Math.multiplyExact(w,h)];for(int i=0;i<pixels.length;i++){int blue=b.get()&255,g=b.get()&255,r=b.get()&255,a=b.get()&255;pixels[i]=Color.argb(a,r,g,blue);}images.put(id,Bitmap.createBitmap(pixels,w,h,Bitmap.Config.ARGB_8888));break;}
            // Canvas metrics measure leftward from the aligned origin, not a signed X coordinate.
            case 5:{CanvasPlayer.font(measure,this,b);int align=b.getInt(),baseline=b.getInt(),count=b.getInt();if(count<0||count>b.remaining()/2)throw new IllegalArgumentException("Invalid text");if(count>text.length)text=new char[Math.max(count,text.length*2)];for(int i=0;i<count;i++)text[i]=b.getChar();float width=measure.measureText(text,0,count);measure.getTextBounds(text,0,count,glyphBounds);measure.getFontMetrics(metrics);float x=CanvasPlayer.align(width,align),y=CanvasPlayer.baseline(metrics,baseline);b.clear();b.putFloat(width).putFloat(-glyphBounds.left-x).putFloat(glyphBounds.right+x).putFloat(-glyphBounds.top-y).putFloat(glyphBounds.bottom+y).putFloat(-metrics.top-y).putFloat(metrics.bottom+y).putFloat(-metrics.ascent).putFloat(metrics.descent).putFloat(y).putFloat(metrics.ascent*.8f+y).putFloat(metrics.descent+y);break;}
            case 6:active(b.getInt()!=0);break;
            case 7:input.sync(b);break;
            case 8:input.keyboard(b.getInt()!=0);break;
            case 9:{int appearance=b.getInt();if(appearance<0||appearance>2)throw new IllegalArgumentException("Invalid status-bar appearance");statusBarAppearance=appearance;applyStatusBarAppearance();break;}
            default:throw new IllegalArgumentException("Unknown bridge operation "+operation);
        }
    }
}
