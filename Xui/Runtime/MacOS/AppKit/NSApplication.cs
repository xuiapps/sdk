using static Xui.Runtime.MacOS.ObjC;
using static Xui.Runtime.MacOS.Marshalling;

namespace Xui.Runtime.MacOS;

public static partial class AppKit
{
    public class NSApplication : NSResponder
    {
        public static new readonly Class Class = new Class(AppKit.Lib, "NSApplication");

        public static readonly Sel SharedApplicationSel = new Sel("sharedApplication");
        public static readonly Sel SetActivationPolicySel = new Sel("setActivationPolicy:");

        public static readonly Sel SetDelegateSel = new Sel("setDelegate:");

        public static readonly Sel ActivateSel = new Sel("activate");

        public static readonly Sel ActivateIgnoringOtherAppsSel = new Sel("activateIgnoringOtherApps:");

        public static readonly Sel RunSel = new Sel("run");

        public static readonly Sel TerminateSel = new Sel("terminate:");

        static NSApplication()
        {
            Marshalling.SetClassWrapper(Class, id => new NSApplication(CoreFoundation.CFRetain(id)));
        }

        private NSApplication(nint id) : base(id)
        {
        }

        public static NSApplication? SharedApplication
        {
            get => ObjCToCSharpNullable<NSApplication>(objc_msgSend_retIntPtr(Class, SharedApplicationSel));
        }

        /// <summary>
        /// An Objecitve-C instance implementing NSApplicationDelegate protocol
        /// </summary>
        public Ref Delegate
        {
            set
            {
                objc_msgSend_retIntPtr(this, SetDelegateSel, value);
            }
        }

        public void Activate()
        {
            objc_msgSend(this, ActivateSel);
        }

        public void Run()
        {
            objc_msgSend(this, RunSel);
        }

        public void Terminate()
        {
            objc_msgSend(this, TerminateSel, (nint)0);
        }

        public override NSApplication Autorelease() => (NSApplication)base.Autorelease();

        public bool SetActivationPolicy(NSApplicationActivationPolicy v) => objc_msgSend_retBool(this, SetActivationPolicySel, (nint)v);
    }
}