using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    public partial class UIResponder : NSObject
    {
        public static new readonly Class Class = new Class(Lib, "UIResponder");

        public static readonly Sel becomeFirstResponderSel = new Sel("becomeFirstResponder");
        private static readonly Sel ReloadInputViewsSel = new("reloadInputViews");

        public UIResponder() : base(Class.New())
        {
        }

        public UIResponder(nint id) : base(id)
        {
        }

        public bool BecomeFirstResponder() =>
            objc_msgSend_retBool(this, becomeFirstResponderSel);

        /// <summary>Refreshes keyboard traits when the focused Xui text client changes.</summary>
        public void ReloadInputViews() => objc_msgSend(this, ReloadInputViewsSel);
    }
}
