using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    /// <summary>Owned, reusable UIKit selection feedback generator.</summary>
    public sealed class UISelectionFeedbackGenerator : NSObject
    {
        public static new readonly Class Class = new(Lib, "UISelectionFeedbackGenerator");
        private static readonly Sel PrepareSel = new("prepare");
        private static readonly Sel SelectionChangedSel = new("selectionChanged");

        public UISelectionFeedbackGenerator() : base(Class.New()) { }

        public void Prepare() => objc_msgSend(this, PrepareSel);
        public void SelectionChanged() => objc_msgSend(this, SelectionChangedSel);
    }
}
