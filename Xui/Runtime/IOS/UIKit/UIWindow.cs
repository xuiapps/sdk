using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.ObjC;

namespace Xui.Runtime.IOS;

public static partial class UIKit
{
    public partial class UIWindow : UIView
    {
        public static new readonly Class Class = new Class(Lib, "UIWindow");

        public static readonly Sel MakeKeyAndVisibleSel = new Sel("makeKeyAndVisible");

        public static readonly Prop.NInt RootViewControllerProp = new Prop.NInt("rootViewController", "setRootViewController:");

        public static readonly Prop.NInt BackgroundColorProp = new Prop.NInt("backgroundColor", "setBackgroundColor:");

        public static readonly Prop.NInt ScreenProp = new Prop.NInt("screen", "setScreen:");

        public UIWindow() : base(Class.New())
        {
        }

        public UIWindow(nint id) : base(id)
        {
        }

        public UIViewController RootViewController
        {
            get => Marshalling.Get<UIViewController>(RootViewControllerProp.Get(this));
            set => RootViewControllerProp.Set(this, value);
        }

        /// <summary>Borrowed UIScreen associated with this window.</summary>
        public nint Screen => ScreenProp.Get(this);

        public void MakeKeyAndVisible() => ObjC.objc_msgSend(this, MakeKeyAndVisibleSel);
    }
}
