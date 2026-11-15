package xui.runtime;

import android.app.Activity;
import android.os.*;
import android.graphics.*;
import android.util.Log;
import android.view.ViewGroup;
import java.nio.*;
import java.io.*;

/** Runs the C#-generated packets through the real C/JNI bridge and Android Canvas/Views. */
public final class CompositionSmoke extends Activity {
    static {System.loadLibrary("xui_android_test");}
    private static native long create(Activity activity);
    private static native void call(long host,int op,ByteBuffer bytes,int length);
    private static native void close(long host);
    private static native void gpuInit(long host);
    private static native int gpuDraws();
    private static native void gpuRender();
    private long nativeHost;
    private Host host;
    private final Handler handler=new Handler(Looper.getMainLooper());
    private void check(boolean condition,String message) {if(!condition)throw new AssertionError(message);}
    private ByteBuffer load(String asset) throws IOException {
        byte[] bytes;try(InputStream in=getAssets().open(asset)){bytes=in.readAllBytes();}
        ByteBuffer b=ByteBuffer.allocateDirect(bytes.length).order(ByteOrder.LITTLE_ENDIAN);b.put(bytes).flip();return b;
    }
    private void send(int op,ByteBuffer bytes) {call(nativeHost,op,bytes,bytes.limit());}
    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        try {
            nativeHost=create(this);check(nativeHost!=0,"native host creation");
            host=(Host)((ViewGroup)findViewById(android.R.id.content)).getChildAt(0);
            ByteBuffer font=ByteBuffer.allocateDirect(128).order(ByteOrder.LITTLE_ENDIAN);
            font.putInt(1).putInt(10);for(char c:"sans-serif".toCharArray())font.putChar(c);
            font.putInt(400).putInt(0).putInt(0).flip();send(2,font);
            ByteBuffer image=ByteBuffer.allocateDirect(16).order(ByteOrder.LITTLE_ENDIAN);
            image.putInt(1).putInt(1).putInt(1).putInt(0xffffffff).flip();send(4,image);
            pixels();textMetrics();
            send(1,load("initial.bin"));
            handler.postDelayed(this::move,400);
        } catch(Throwable failure) {fail(failure);}
    }
    private Surface child() {return (Surface)((Surface)host.getChildAt(0)).getChildAt(0);}
    private void statusBar() {
        ByteBuffer packet=ByteBuffer.allocateDirect(4).order(ByteOrder.LITTLE_ENDIAN);
        for(int appearance:new int[]{1,2,0}) {
            packet.clear();packet.putInt(appearance).flip();send(9,packet);
            boolean dark=appearance==2 || (appearance==0 &&
                (getResources().getConfiguration().uiMode & android.content.res.Configuration.UI_MODE_NIGHT_MASK)!=android.content.res.Configuration.UI_MODE_NIGHT_YES);
            boolean actual;
            if(Build.VERSION.SDK_INT>=30) actual=(getWindow().getInsetsController().getSystemBarsAppearance() &
                android.view.WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS)!=0;
            else actual=(getWindow().getDecorView().getSystemUiVisibility() & android.view.View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR)!=0;
            check(actual==dark,"status-bar foreground appearance "+appearance);
        }
    }
    private int attachAttempts;
    private void move() {
        try {
            statusBar();
            Surface node=child();
            if((!node.isAttachedToWindow()||node.player.replayCount==0)&&attachAttempts++<100){handler.postDelayed(this::move,100);return;}
            check(node.isHardwareAccelerated(),"hardware canvas required");
            long before=node.player.replayCount;check(before>0,"initial content replay");
            int id=node.id;send(1,load("moved.bin"));
            handler.postDelayed(()->{
                try {
                    check(child()==node&&child().id==id,"stable native view identity");
                    check(node.player.replayCount==before,"moving cached content must not replay");
                    // Equal-content upload must not invalidate either.
                    send(1,load("initial.bin"));
                    handler.postDelayed(()->{
                        try {check(node.player.replayCount==before,"identical graphics upload must not replay");shadows();}
                        catch(Throwable failure){fail(failure);}
                    },200);
                } catch(Throwable failure){fail(failure);}
            },200);
        } catch(Throwable failure){fail(failure);}
    }
    private void eventually(java.util.function.BooleanSupplier ready,Runnable action,int remaining) {
        try {
            if(ready.getAsBoolean())action.run();
            else if(remaining>0)handler.postDelayed(()->eventually(ready,action,remaining-1),50);
            else throw new AssertionError("Timed out waiting for native GPU presentation");
        }catch(Throwable failure){fail(failure);}
    }
    private void shadowPixels(java.util.function.IntConsumer result) {
        Surface card=child();int[] location=new int[2];card.getLocationInWindow(location);
        Rect band=new Rect(location[0]+20,location[1]+card.getHeight()+1,
            location[0]+card.getWidth()-20,location[1]+card.getHeight()+10);
        Bitmap bitmap=Bitmap.createBitmap(band.width(),band.height(),Bitmap.Config.ARGB_8888);
        android.view.PixelCopy.request(getWindow(),band,bitmap,status->{
            try {
                check(status==android.view.PixelCopy.SUCCESS,"hardware shadow capture");
                int darkest=255;
                for(int y=0;y<bitmap.getHeight();y++)for(int x=0;x<bitmap.getWidth();x++)
                    darkest=Math.min(darkest,Color.red(bitmap.getPixel(x,y)));
                result.accept(darkest);
            }catch(Throwable failure){fail(failure);}finally{bitmap.recycle();}
        },handler);
    }
    private void shadows() throws IOException {
        send(1,load("shadow-none.bin"));
        handler.postDelayed(()->shadowPixels(none->{
            try {
                check(none==255,"no shadow leaves the area below the card white: "+none);
                send(1,load("shadow.bin"));
                handler.postDelayed(()->shadowPixels(shadow->{
                    try {
                        check(shadow<=251,"subtle outline shadow must remain visible: "+shadow);
                        check(shadow>=230,"outline shadow must remain subtle: "+shadow);
                        gpu();
                    }catch(Throwable failure){fail(failure);}
                }),200);
            }catch(Throwable failure){fail(failure);}
        }),200);
    }
    private void gpu() throws IOException {
        gpuInit(nativeHost);send(1,load("gpu.bin"));
        eventually(()->((android.view.TextureView)child().getChildAt(0)).isAvailable(),()->gpuRender(),100);
        eventually(()->child().gpuFrameCount>0,()->{
            try {
                Surface node=child();android.view.TextureView texture=(android.view.TextureView)node.getChildAt(0);
                Bitmap frame=texture.getBitmap();check(frame!=null,"Vulkan frame available");
                check(frame.getPixel(frame.getWidth()/2,frame.getHeight()/2)==Color.RED,"Vulkan shader uniforms/locations and front-face culling: center="+Integer.toHexString(frame.getPixel(frame.getWidth()/2,frame.getHeight()/2))+", corner="+Integer.toHexString(frame.getPixel(2,2))+", draws="+gpuDraws()+", frames="+node.gpuFrameCount);
                check(frame.getPixel(frame.getWidth()*85/100,frame.getHeight()*77/100)==Color.RED,"row-major translation uniform");
                check(Math.abs(Color.red(frame.getPixel(2,2))-128)<=1,"Vulkan clear color");frame.recycle();
                check(gpuDraws()==1,"one initial Vulkan submission");send(1,load("gpu-moved.bin"));
                handler.postDelayed(()->{
                    try {check(child()==node,"GPU view reused");check(gpuDraws()==1,"transform-only GPU movement reuses last frame");send(1,load("initial.bin"));
                        handler.postDelayed(()->{
                            try {send(1,load("gpu.bin"));eventually(()->((android.view.TextureView)child().getChildAt(0)).isAvailable(),()->gpuRender(),100);handler.postDelayed(()->{
                                try {check(gpuDraws()==2,"GPU surface recreated after culling");Log.i("XuiCompositionTest","PASS: pixels, alpha, state, JNI buffers, hardware view reuse, Vulkan pixels and lifecycle");}
                                catch(Throwable failure){fail(failure);}
                            },500);}catch(Throwable failure){fail(failure);}
                        },300);
                    }catch(Throwable failure){fail(failure);}
                },300);
            }catch(Throwable failure){fail(failure);}
        },100);
    }
    private void pixels() throws IOException {
        Bitmap bitmap=Bitmap.createBitmap(100,50,Bitmap.Config.ARGB_8888);Canvas canvas=new Canvas(bitmap);
        CanvasPlayer player=new CanvasPlayer(host);player.replay(canvas,load("drawing.bin"));
        check(bitmap.getPixel(5,5)==Color.RED,"fill rect");
        check(Math.abs(Color.alpha(bitmap.getPixel(25,5))-64)<=1,"global alpha times image opacity");
        check(bitmap.getPixel(45,5)==Color.BLUE,"saved paint");
        check(Math.abs(Color.alpha(bitmap.getPixel(65,5))-128)<=1&&Color.red(bitmap.getPixel(65,5))==255,"restored paint and alpha");
        Bitmap expected=Bitmap.createBitmap(100,50,Bitmap.Config.ARGB_8888);Canvas reference=new Canvas(expected);
        Paint dash=new Paint(Paint.ANTI_ALIAS_FLAG);dash.setColor(Color.WHITE);dash.setAlpha(128);dash.setStyle(Paint.Style.STROKE);dash.setStrokeWidth(2);dash.setPathEffect(new DashPathEffect(new float[]{2,4},1));
        reference.drawLine(0,30,80,30,dash);
        for(int x=0;x<80;x++)check(bitmap.getPixel(x,30)==expected.getPixel(x,30),"dash Save/Restore at "+x);
        bitmap.recycle();expected.recycle();
    }
    private void textMetrics() {
        String sample="Hello World!";
        Paint paint=new Paint(Paint.ANTI_ALIAS_FLAG);
        paint.setTypeface(host.font(1));paint.setTextSize(32);paint.setColor(Color.BLACK);
        for(int align:new int[]{0,1,2,3,4}) {
            ByteBuffer packet=ByteBuffer.allocateDirect(128).order(ByteOrder.LITTLE_ENDIAN);
            packet.putInt(1).putFloat(32).putFloat(1).putFloat(0);
            packet.putInt(align).putInt(3).putInt(sample.length());
            for(char c:sample.toCharArray())packet.putChar(c);
            packet.flip();send(5,packet);
            float left=packet.getFloat(4),right=packet.getFloat(8);
            float ascent=packet.getFloat(12),descent=packet.getFloat(16);
            check(left+right>100,"actual glyph width must survive alignment "+align);
            Bitmap bitmap=Bitmap.createBitmap(500,150,Bitmap.Config.ARGB_8888);
            paint.setTextAlign(align==4?Paint.Align.CENTER:align==1||align==3?Paint.Align.RIGHT:Paint.Align.LEFT);
            new Canvas(bitmap).drawText(sample,250,100,paint);
            int minX=500,maxX=-1,minY=150,maxY=-1;
            for(int y=0;y<150;y++)for(int x=0;x<500;x++)if(Color.alpha(bitmap.getPixel(x,y))!=0) {
                minX=Math.min(minX,x);maxX=Math.max(maxX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);
            }
            check(Math.abs((250-left)-minX)<=2&&Math.abs((250+right)-(maxX+1))<=2,
                "actual glyph horizontal bounds match rendered pixels, alignment "+align);
            check(Math.abs((100-ascent)-minY)<=2&&Math.abs((100+descent)-(maxY+1))<=2,
                "actual glyph vertical bounds match rendered pixels");
            bitmap.recycle();
        }
    }
    private void fail(Throwable failure){Log.e("XuiCompositionTest","FAIL",failure);}
    @Override protected void onDestroy(){handler.removeCallbacksAndMessages(null);if(nativeHost!=0){close(nativeHost);nativeHost=0;}super.onDestroy();}
}
