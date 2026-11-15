using Android.Content;
using Android.Graphics;
using Xui.Core.Math2D;

namespace Xui.Runtime.Android.Actual;

public class XuiUI : global::Android.Views.View
{
    internal AndroidHapticFeedback Haptics { get; }
    private AndroidVibration? vibration;
    internal AndroidVibration Vibration => vibration ??= new AndroidVibration(this);
    internal void StopVibration() => vibration?.Stop();
    internal void DisposeVibration() => vibration?.Dispose();

    private static readonly Java.Lang.Class baseCanvasClass;
    private static readonly Java.Lang.Reflect.Field mNativeCanvasWrapperField;

    static XuiUI()
    {
        baseCanvasClass = Java.Lang.Class.ForName("android.graphics.BaseCanvas");
        mNativeCanvasWrapperField = baseCanvasClass.GetDeclaredField("mNativeCanvasWrapper");
        mNativeCanvasWrapperField.Accessible = true;
    }

    protected readonly XuiActivity xuiActivity;
    private Vector size = (0, 0);

    public XuiUI(XuiActivity xuiActivity) : base(xuiActivity)
    {
        this.xuiActivity = xuiActivity;
        Haptics = new AndroidHapticFeedback(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { Haptics.Dispose(); vibration?.Dispose(); }
        base.Dispose(disposing);
    }

    protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
    {
        this.size = (w, h);
        base.OnSizeChanged(w, h, oldw, oldh);
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);

        long nCanvas = mNativeCanvasWrapperField.GetLong(canvas);
        // TODO: For performance reasons, use PInvoke over a flat lib of the Android built-in Skia canvas.
        // TODO: Setup drawing context over the Android canvas...

        this.xuiActivity.OnDraw(this.size, canvas);
    }
}
