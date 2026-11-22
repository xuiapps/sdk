package xui.runtime;

import android.graphics.*;
import android.os.Build;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

/** Decodes Xui's platform wire stream into a hardware recording Canvas. No bitmap snapshot. */
final class CanvasPlayer {
    private final Host host;
    private final Path path = new Path(), scratch = new Path();
    private final Matrix matrix = new Matrix(), old = new Matrix(), inverse = new Matrix();
    private final float[] values = new float[9], points = new float[4], radii = new float[8];
    private final RectF rect = new RectF(), source = new RectF();
    private final Paint imagePaint = new Paint(Paint.ANTI_ALIAS_FLAG | Paint.FILTER_BITMAP_FLAG);
    private final Paint.FontMetrics metrics = new Paint.FontMetrics();
    private final ArrayList<State> stack = new ArrayList<>();
    private final State state = new State();
    private int depth;
    private float px, py, sx, sy;
    private char[] text = new char[64];
    long replayCount;

    CanvasPlayer(Host host) { this.host = host; }
    static final class State {
        final Matrix transform = new Matrix();
        final Paint fill = new Paint(Paint.ANTI_ALIAS_FLAG), stroke = new Paint(Paint.ANTI_ALIAS_FLAG);
        float alpha = 1, brightness = 1, dashOffset;
        float[] dashes;
        int align, baseline = 3;
        State() { reset(); }
        void reset() {
            transform.reset(); fill.reset(); stroke.reset(); fill.setAntiAlias(true); stroke.setAntiAlias(true);
            fill.setColor(Color.BLACK); stroke.setColor(Color.BLACK); stroke.setStyle(Paint.Style.STROKE);
            stroke.setStrokeWidth(1); stroke.setStrokeMiter(10);
            alpha = brightness = 1; dashOffset = 0; dashes = null; align = 0; baseline = 3;
        }
        void copy(State other) {
            transform.set(other.transform); fill.set(other.fill); stroke.set(other.stroke); alpha = other.alpha; brightness = other.brightness;
            dashOffset = other.dashOffset; dashes = other.dashes; align = other.align; baseline = other.baseline;
        }
    }
    private static float clamp(float n) { return Math.max(0, Math.min(1, n)); }
    static int color(ByteBuffer b, float brightness) {
        float r=b.getFloat(), g=b.getFloat(), blue=b.getFloat(), a=b.getFloat();
        return Color.argb(Math.round(clamp(a)*255), Math.round(clamp(r*brightness)*255), Math.round(clamp(g*brightness)*255), Math.round(clamp(blue*brightness)*255));
    }
    private void rect(ByteBuffer b, RectF r) { float x=b.getFloat(), y=b.getFloat(); r.set(x,y,x+b.getFloat(),y+b.getFloat()); }
    private void readMatrix(ByteBuffer b) {
        values[0]=b.getFloat(); values[3]=b.getFloat(); values[1]=b.getFloat(); values[4]=b.getFloat();
        values[2]=b.getFloat(); values[5]=b.getFloat(); values[6]=values[7]=0; values[8]=1; matrix.setValues(values);
    }
    // Canvas paths survive state changes: move their coordinates into the new user space.
    private void rebase(Canvas c) {
        if (!state.transform.invert(inverse)) throw new UnsupportedOperationException("Singular path transform");
        inverse.preConcat(old); path.transform(inverse);
        points[0]=px; points[1]=py; points[2]=sx; points[3]=sy; inverse.mapPoints(points);
        px=points[0]; py=points[1]; sx=points[2]; sy=points[3];
    }
    private void dash() { state.stroke.setPathEffect(state.dashes == null ? null : new DashPathEffect(state.dashes, state.dashOffset)); }
    private void paintColor(Paint p, ByteBuffer b) { p.setShader(null); p.setColor(color(b,state.brightness)); }
    private Shader gradient(ByteBuffer b, boolean radial) {
        float x0=b.getFloat(), y0=b.getFloat(), r0=radial ? b.getFloat() : 0;
        float x1=b.getFloat(), y1=b.getFloat(), r1=radial ? b.getFloat() : 0;
        int count=b.getInt(); if(count<2 || count>b.remaining()/20) throw new IllegalArgumentException("Invalid gradient stops");
        float[] offsets=new float[count]; int[] colors=new int[count];
        for(int i=0;i<count;i++) { offsets[i]=b.getFloat(); colors[i]=color(b,state.brightness); }
        if(!radial) return new LinearGradient(x0,y0,x1,y1,colors,offsets,Shader.TileMode.CLAMP);
        if(Build.VERSION.SDK_INT>=31) { long[] wide=new long[count];for(int i=0;i<count;i++)wide[i]=Color.pack(colors[i]);return new RadialGradient(x0,y0,r0,x1,y1,r1,wide,offsets,Shader.TileMode.CLAMP); }
        if(r0!=0 || x0!=x1 || y0!=y1) throw new UnsupportedOperationException("Two-circle radial gradients require Android 12");
        return new RadialGradient(x1,y1,r1,colors,offsets,Shader.TileMode.CLAMP);
    }
    private void shader(Paint p, Shader shader) { p.setColor(Color.BLACK); p.setShader(shader); }
    private void pattern(Paint p, ByteBuffer b) {
        Bitmap image=host.image(b.getInt()); int repeat=b.getInt();
        shader(p,new BitmapShader(image, repeat==0||repeat==1 ? Shader.TileMode.REPEAT:Shader.TileMode.CLAMP,
            repeat==0||repeat==2 ? Shader.TileMode.REPEAT:Shader.TileMode.CLAMP));
    }
    static void font(Paint p, Host host, ByteBuffer b) {
        p.setTypeface(host.font(b.getInt())); p.setTextSize(b.getFloat()); p.setTextScaleX(b.getFloat());
        p.setTextSkewX(-(float)Math.tan(Math.toRadians(b.getFloat())));
    }
    static float baseline(Paint.FontMetrics m, int baseline) {
        switch(baseline) { case 0: return -m.ascent; case 1: return -m.ascent*.8f;
            case 2:return -(m.ascent+m.descent)/2; case 4: case 5:return -m.descent; default:return 0; }
    }
    static float align(float width, int align) { return align==1||align==3 ? -width : align==4 ? -width/2 : 0; }
    private void text(Canvas c, ByteBuffer b) {
        float x=b.getFloat(),y=b.getFloat(); int count=b.getInt();
        if(count<0 || count>b.remaining()/2) throw new IllegalArgumentException("Invalid text length");
        if(count>text.length) text=new char[Math.max(count,text.length*2)];
        for(int i=0;i<count;i++) text[i]=b.getChar();
        float width=state.fill.measureText(text,0,count); state.fill.getFontMetrics(metrics);
        int alpha=state.fill.getAlpha(); state.fill.setAlpha(Math.round(alpha*clamp(state.alpha)));
        c.drawText(text,0,count,x+align(width,state.align),y+baseline(metrics,state.baseline),state.fill);
        state.fill.setAlpha(alpha);
    }
    private void ellipse(ByteBuffer b) {
        float cx=b.getFloat(), cy=b.getFloat(), rx=b.getFloat(), ry=b.getFloat(), rotation=b.getFloat();
        float start=b.getFloat(), end=b.getFloat(); boolean clockwise=b.getInt()!=0;
        if(rx<0||ry<0) throw new IllegalArgumentException("Negative ellipse radius");
        double tau=Math.PI*2, sweep=end-start;
        if(clockwise) { sweep=sweep>=tau?tau:(sweep%tau+tau)%tau; }
        else { sweep=-sweep>=tau?-tau:-((-sweep%tau+tau)%tau); }
        int steps=Math.max(1,(int)Math.ceil(Math.abs(sweep)/(Math.PI/2)));
        double cos=Math.cos(rotation),sin=Math.sin(rotation), a=start;
        float x=(float)(cx+rx*Math.cos(a)*cos-ry*Math.sin(a)*sin),y=(float)(cy+rx*Math.cos(a)*sin+ry*Math.sin(a)*cos);
        if(path.isEmpty()) { path.moveTo(x,y); sx=x; sy=y; } else path.lineTo(x,y);
        for(int i=0;i<steps;i++) {
            double next=start+sweep*(i+1)/steps,k=4.0/3*Math.tan((next-a)/4);
            float nx=(float)(cx+rx*Math.cos(next)*cos-ry*Math.sin(next)*sin),ny=(float)(cy+rx*Math.cos(next)*sin+ry*Math.sin(next)*cos);
            path.cubicTo(x+(float)(k*(-rx*Math.sin(a)*cos-ry*Math.cos(a)*sin)),y+(float)(k*(-rx*Math.sin(a)*sin+ry*Math.cos(a)*cos)),
                nx-(float)(k*(-rx*Math.sin(next)*cos-ry*Math.cos(next)*sin)),ny-(float)(k*(-rx*Math.sin(next)*sin+ry*Math.cos(next)*cos)),nx,ny);
            a=next; x=nx; y=ny;
        }
        px=x; py=y;
    }
    private void arcTo(ByteBuffer b) {
        float x=b.getFloat(),y=b.getFloat(),ex=b.getFloat(),ey=b.getFloat(),r=b.getFloat();
        if(r<0) throw new IllegalArgumentException("Negative arc radius");
        if(path.isEmpty()) {path.moveTo(x,y);px=sx=x;py=sy=y;return;}
        double ax=px-x,ay=py-y,bx=ex-x,by=ey-y,al=Math.hypot(ax,ay),bl=Math.hypot(bx,by);
        if(al==0||bl==0||r==0) {path.lineTo(x,y);px=x;py=y;return;}
        ax/=al;ay/=al;bx/=bl;by/=bl;
        double cross=ax*by-ay*bx,angle=Math.acos(Math.max(-1,Math.min(1,ax*bx+ay*by)));
        if(Math.abs(cross)<1e-6) {path.lineTo(x,y);px=x;py=y;return;}
        double distance=r/Math.tan(angle/2),tx=x+ax*distance,ty=y+ay*distance;
        double ux=x+bx*distance,uy=y+by*distance,sign=cross<0?-1:1;
        double cx=tx-sign*ay*r,cy=ty+sign*ax*r;
        float start=(float)Math.toDegrees(Math.atan2(ty-cy,tx-cx));
        float sweep=(float)(-sign*(180-Math.toDegrees(angle)));
        path.lineTo((float)tx,(float)ty); rect.set((float)cx-r,(float)cy-r,(float)cx+r,(float)cy+r);path.arcTo(rect,start,sweep);
        px=(float)ux;py=(float)uy;
    }
    private void draw(Canvas c, Paint p, boolean rectangle) {
        int alpha=p.getAlpha();p.setAlpha(Math.round(alpha*clamp(state.alpha)));
        if(rectangle)c.drawRect(rect,p);else c.drawPath(path,p); p.setAlpha(alpha);
    }
    void replay(Canvas c, ByteBuffer b) { replay(c,b,1); }
    void replay(Canvas c, ByteBuffer b, float brightness) {
        replayCount++; state.reset(); state.brightness=brightness; depth=0; path.reset(); px=py=sx=sy=0;
        int save=c.save(); b.position(0);
        try {
            while(b.hasRemaining()) {
                int op=b.getInt();
                switch(op) {
                    case DrawOp.Symbol:host.drawSymbol(c,b.getInt(),state.brightness);path.reset();px=py=sx=sy=0;break;
                    case DrawOp.Save: if(depth==stack.size())stack.add(new State());stack.get(depth++).copy(state);c.save();break;
                    case DrawOp.Restore: if(depth==0)throw new IllegalArgumentException("Unbalanced Restore");old.set(state.transform);c.restore();state.copy(stack.get(--depth));rebase(c);break;
                    case DrawOp.BeginPath:path.reset();px=py=sx=sy=0;break;
                    case DrawOp.ClosePath:path.close();px=sx;py=sy;break;
                    case DrawOp.Clip:c.clipPath(path);break;
                    case DrawOp.GlobalAlpha:state.alpha=b.getFloat();break;
                    case DrawOp.LineWidth:state.stroke.setStrokeWidth(b.getFloat());break;
                    case DrawOp.MiterLimit:state.stroke.setStrokeMiter(b.getFloat());break;
                    case DrawOp.LineCap:{int cap=b.getInt();state.stroke.setStrokeCap(cap==0?Paint.Cap.BUTT:cap==1?Paint.Cap.ROUND:Paint.Cap.SQUARE);break;}
                    case DrawOp.LineJoin:{int join=b.getInt();state.stroke.setStrokeJoin(join==0?Paint.Join.MITER:join==1?Paint.Join.ROUND:Paint.Join.BEVEL);break;}
                    case DrawOp.LineDashOffset:state.dashOffset=b.getFloat();dash();break;
                    case DrawOp.Dashes:{int n=b.getInt();if(n<0||n>b.remaining()/4)throw new IllegalArgumentException("Invalid dashes");state.dashes=n==0?null:new float[n*(n%2==0?1:2)];for(int i=0;i<n;i++)state.dashes[i]=b.getFloat();if(n%2!=0)System.arraycopy(state.dashes,0,state.dashes,n,n);dash();break;}
                    case DrawOp.TextAlign:state.align=b.getInt();break;
                    case DrawOp.TextBaseline:state.baseline=b.getInt();break;
                    case DrawOp.Translate:{old.set(state.transform);float x=b.getFloat(),y=b.getFloat();c.translate(x,y);state.transform.preTranslate(x,y);rebase(c);break;}
                    case DrawOp.Scale:{old.set(state.transform);float x=b.getFloat(),y=b.getFloat();c.scale(x,y);state.transform.preScale(x,y);rebase(c);break;}
                    case DrawOp.Rotate:{old.set(state.transform);float angle=(float)Math.toDegrees(b.getFloat());c.rotate(angle);state.transform.preRotate(angle);rebase(c);break;}
                    case DrawOp.Transform:old.set(state.transform);readMatrix(b);c.concat(matrix);state.transform.preConcat(matrix);rebase(c);break;
                    case DrawOp.SetTransform:old.set(state.transform);if(!old.invert(inverse))throw new UnsupportedOperationException("Singular transform");c.concat(inverse);readMatrix(b);c.concat(matrix);state.transform.set(matrix);rebase(c);break;
                    case DrawOp.MoveTo:px=sx=b.getFloat();py=sy=b.getFloat();path.moveTo(px,py);break;
                    case DrawOp.LineTo:px=b.getFloat();py=b.getFloat();path.lineTo(px,py);break;
                    case DrawOp.Quadratic:{float x=b.getFloat(),y=b.getFloat();px=b.getFloat();py=b.getFloat();path.quadTo(x,y,px,py);break;}
                    case DrawOp.Cubic:{float x=b.getFloat(),y=b.getFloat(),x2=b.getFloat(),y2=b.getFloat();px=b.getFloat();py=b.getFloat();path.cubicTo(x,y,x2,y2,px,py);break;}
                    case DrawOp.ArcTo:arcTo(b);break;
                    case DrawOp.Ellipse:ellipse(b);break;
                    case DrawOp.Rect:rect(b,rect);path.addRect(rect,Path.Direction.CW);px=sx=rect.left;py=sy=rect.top;break;
                    case DrawOp.RoundRect:rect(b,rect);for(int i=0;i<8;i+=2)radii[i]=radii[i+1]=b.getFloat();path.addRoundRect(rect,radii,Path.Direction.CW);px=sx=rect.left+radii[0];py=sy=rect.top;break;
                    case DrawOp.Fill:path.setFillType(b.getInt()==0?Path.FillType.WINDING:Path.FillType.EVEN_ODD);draw(c,state.fill,false);break;
                    case DrawOp.Stroke:draw(c,state.stroke,false);break;
                    case DrawOp.FillRect:rect(b,rect);draw(c,state.fill,true);break;
                    case DrawOp.StrokeRect:rect(b,rect);draw(c,state.stroke,true);break;
                    case DrawOp.FillColor:paintColor(state.fill,b);break;
                    case DrawOp.StrokeColor:paintColor(state.stroke,b);break;
                    case DrawOp.FilterNone:state.brightness=1;break;
                    case DrawOp.Brightness:state.brightness=b.getFloat();break;
                    case DrawOp.FillLinear:shader(state.fill,gradient(b,false));break;
                    case DrawOp.StrokeLinear:shader(state.stroke,gradient(b,false));break;
                    case DrawOp.FillRadial:shader(state.fill,gradient(b,true));break;
                    case DrawOp.StrokeRadial:shader(state.stroke,gradient(b,true));break;
                    case DrawOp.FillPattern:pattern(state.fill,b);break;
                    case DrawOp.StrokePattern:pattern(state.stroke,b);break;
                    case DrawOp.Font:font(state.fill,host,b);break;
                    case DrawOp.Text:text(c,b);break;
                    case DrawOp.Image:case DrawOp.ImageCrop:{Bitmap image=host.image(b.getInt());if(op==DrawOp.ImageCrop)rect(b,source);else source.set(0,0,image.getWidth(),image.getHeight());rect(b,rect);float alpha=b.getFloat();imagePaint.setAlpha(Math.round(255*clamp(alpha)*clamp(state.alpha)));int imageSave=c.save();c.clipRect(rect);matrix.setRectToRect(source,rect,Matrix.ScaleToFit.FILL);c.drawBitmap(image,matrix,imagePaint);c.restoreToCount(imageSave);break;}
                    default:throw new IllegalArgumentException("Unknown Xui canvas opcode "+op);
                }
            }
            // Inherited node state may contain ancestor Saves. The outer scope
            // restores all of them; Xui validates balance at the frame boundary.
        } finally { c.restoreToCount(save); }
    }
}
