using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.Foundation;
using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class CoreAnimation
{
    public partial class CADisplayLink : NSObject
    {
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FrameRateRange
        {
            public FrameRateRange(float minimum, float maximum, float preferred)
            {
                this.Minimum = minimum;
                this.Maximum = maximum;
                this.Preferred = preferred;
            }

            public float Minimum { get; }
            public float Maximum { get; }
            public float Preferred { get; }
        }

        public static new readonly Class Class = new Class(Lib, "CADisplayLink");

        public static readonly Sel TimestampSel = new Sel("timestamp");

        public static readonly Sel TargetTimestampSel = new Sel("targetTimestamp");

        public static readonly Sel SetPreferredFramesPerSecondSel = new Sel("setPreferredFramesPerSecond:");

        public static readonly Sel SetPreferredFrameRateRangeSel = new Sel("setPreferredFrameRateRange:");

        public static readonly Sel DisplayLinkWithTargetSelector = new Sel("displayLinkWithTarget:selector:");

        public static readonly Sel AddToRunLoopForModeSel = new Sel("addToRunLoop:forMode:");

        public static readonly Sel RemoveFromRunLoopForModeSel = new Sel("removeFromRunLoop:forMode:");

        public CADisplayLink(nint id) : base(id)
        {
        }

        public TimeSpan Timestamp => TimeSpan.FromSeconds(objc_msgSend_retDouble(this, TimestampSel));

        public TimeSpan TargetTimestamp => TimeSpan.FromSeconds(objc_msgSend_retDouble(this, TargetTimestampSel));

        /// <summary>Sets the preferred display-link callback frequency in hertz.</summary>
        public int PreferredFramesPerSecond
        {
            set => ObjC.objc_msgSend(this, SetPreferredFramesPerSecondSel, value);
        }

        /// <summary>
        /// Requests an iOS 15+ callback-rate range. The system treats this as a
        /// preference, and may still select a lower rate for power, thermal, or
        /// accessibility policy.
        /// </summary>
        public FrameRateRange PreferredFrameRateRange
        {
            set => objc_msgSend(this, SetPreferredFrameRateRangeSel, value);
        }

        [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
        private static partial void objc_msgSend(nint obj, nint sel, FrameRateRange value);

        public void AddToRunLoopForMode(NSRunLoop runloop, NSRunLoop.Mode mode) => ObjC.objc_msgSend(this, AddToRunLoopForModeSel, runloop, mode);

        public void RemoveToRunLoopForMode(NSRunLoop runloop, NSRunLoop.Mode mode) => ObjC.objc_msgSend(this, RemoveFromRunLoopForModeSel, runloop, mode);

        public static CADisplayLink DisplayLink(nint target, nint selector) =>
            new CADisplayLink(ObjC.objc_msgSend_retIntPtr(Class, DisplayLinkWithTargetSelector, target, selector));
    }
}
