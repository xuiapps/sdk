using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    public partial class UIViewController : UIResponder
    {
        public static new readonly Class Class = new Class(Lib, "UIViewController");

        public static readonly Prop.NInt ViewProp = new Prop.NInt("view", "setView:");

        public UIViewController() : base(Class.New())
        {
        }

        public UIViewController(nint id) : base(id)
        {
        }

        public void SetNeedsStatusBarAppearanceUpdate() =>
            objc_msgSend(this, new Sel("setNeedsStatusBarAppearanceUpdate"));

        public UIView View
        {
            get => Marshalling.Get<UIView>(ViewProp.Get(this));
            set => ViewProp.Set(this, value);
        }
    }
}