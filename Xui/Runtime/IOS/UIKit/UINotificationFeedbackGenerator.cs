using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    /// <summary>Owned, reusable UIKit notification feedback generator.</summary>
    public sealed class UINotificationFeedbackGenerator : NSObject
    {
        public static new readonly Class Class = new(Lib, "UINotificationFeedbackGenerator");
        private static readonly Sel PrepareSel = new("prepare");
        private static readonly Sel NotificationOccurredSel = new("notificationOccurred:");

        public enum FeedbackType : long { Success, Warning, Error }

        public UINotificationFeedbackGenerator() : base(Class.New()) { }

        public void Prepare() => objc_msgSend(this, PrepareSel);
        public void NotificationOccurred(FeedbackType type) => objc_msgSend(this, NotificationOccurredSel, (nint)type);
    }
}
