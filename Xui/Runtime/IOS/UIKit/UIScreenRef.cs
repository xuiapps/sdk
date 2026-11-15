using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    /// <summary>A borrowed screen reference. Does not retain or release the native screen.</summary>
    public readonly ref struct UIScreenRef
    {
        private static readonly Sel MaximumFramesPerSecondSel = new("maximumFramesPerSecond");

        public readonly nint Self;

        public UIScreenRef(nint self)
        {
            if (self == 0)
                throw new ObjCException($"{nameof(UIScreenRef)} instantiated with nil self.");

            Self = self;
        }

        /// <summary>The highest refresh rate supported by this screen, in hertz.</summary>
        public nint MaximumFramesPerSecond => objc_msgSend_retIntPtr(Self, MaximumFramesPerSecondSel);
    }
}
