using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    /// <summary>Owned UIKit impact generator, retained for reuse with one style.</summary>
    public sealed class UIImpactFeedbackGenerator : NSObject
    {
        public static new readonly Class Class = new(Lib, "UIImpactFeedbackGenerator");
        private static readonly Sel InitWithStyleSel = new("initWithStyle:");
        private static readonly Sel PrepareSel = new("prepare");
        private static readonly Sel ImpactOccurredSel = new("impactOccurred");

        public enum FeedbackStyle : long { Light, Medium, Heavy, Soft, Rigid }

        public UIImpactFeedbackGenerator(FeedbackStyle style)
            : base(objc_msgSend_retIntPtr(Class.Alloc(), InitWithStyleSel, (nint)style)) { }

        public void Prepare() => objc_msgSend(this, PrepareSel);
        public void ImpactOccurred() => objc_msgSend(this, ImpactOccurredSel);
    }
}
