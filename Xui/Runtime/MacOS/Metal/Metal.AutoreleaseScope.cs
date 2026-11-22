using System.Runtime.InteropServices;

namespace Xui.Runtime.MacOS;

public static partial class Metal
{
    // Resource preparation may run on a .NET worker without a UIKit/AppKit autorelease pool.
    internal ref struct AutoreleaseScope
    {
        private nint pool;
        public AutoreleaseScope() => pool = AutoreleasePoolPush();
        public void Dispose()
        {
            if (pool == 0) return;
            AutoreleasePoolPop(pool);
            pool = 0;
        }
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_autoreleasePoolPush")]
    private static partial nint AutoreleasePoolPush();
    [LibraryImport(ObjCLib, EntryPoint = "objc_autoreleasePoolPop")]
    private static partial void AutoreleasePoolPop(nint pool);
}
