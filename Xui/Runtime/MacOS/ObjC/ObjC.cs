using System.Runtime.InteropServices;
using static Xui.Runtime.MacOS.CoreGraphics;
using static Xui.Runtime.MacOS.Foundation;

namespace Xui.Runtime.MacOS;

public static partial class ObjC
{
    public const string LibObjCLib = "/usr/lib/libobjc.A.dylib";

    public static readonly nint Lib;

    static ObjC()
    {
        Lib = NativeLibrary.Load(LibObjCLib);
    }

    [LibraryImport(LibObjCLib, EntryPoint="object_getClass")]
    public static partial nint object_getClass(nint instance);


    public static string? object_getClassName(nint instance)
    {
        if (instance == 0)
        {
            return null;
        }
        nint charPtr = object_getClassName_retIntPtr(instance);
        if (charPtr == 0)
        {
            return null;
        }
        return Marshal.PtrToStringAnsi(charPtr);
    }

    [LibraryImport(LibObjCLib, EntryPoint="object_getClassName")]
    private static partial nint object_getClassName_retIntPtr(nint instance);

    [LibraryImport(LibObjCLib, EntryPoint="objc_getClass")]
    public static partial nint objc_getClass([MarshalAs(UnmanagedType.LPStr)] string name);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial nint objc_msgSend_retIntPtr(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial nint objc_msgSend_retIntPtr(nint obj, nint sel, nint id1, nint id2);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial double objc_msgSend_retDouble(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string objc_msgSend_retCStr(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial nuint objc_msgSend_retNUInt(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial NFloat objc_msgSend_retNFloat(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial float objc_msgSend_retFloat(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial nint objc_msgSend_retIntPtr(nint obj, nint sel, nint id1);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial int objc_msgSend_retInt(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static unsafe partial CGSize objc_msgSend_retCGSize(nint obj, nint sel, nint id1);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static unsafe partial NSRect objc_msgSend_retNSRect(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static unsafe partial NSRect objc_msgSend_retNSRect(nint obj, nint sel, NSRect rect);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel, NFloat v);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel, nuint v);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel, nint a1, uint v2);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel, nint a1, nint v2);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel, [MarshalAs(UnmanagedType.I1)] bool v1);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel, nint id1);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSendSuper")]
    public static partial void objc_msgSendSuper(ref Super obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSendSuper")]
    public static partial void objc_msgSendSuper(ref Super obj, nint sel, NSRect rect);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSendSuper")]
    public static partial void objc_msgSendSuper(ref Super obj, nint sel, nint id1);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    public static partial void objc_msgSend(nint obj, nint sel, int id1);

    [LibraryImport(LibObjCLib)]
    public static partial nint objc_getProtocol([MarshalAs(UnmanagedType.LPStr)] string name);

    [LibraryImport(LibObjCLib)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool class_addProtocol(nint objcclass, nint name);

    [LibraryImport(LibObjCLib)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string class_getName(nint objcclass);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool objc_msgSend_retBool(nint obj, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool objc_msgSend_retBool(nint obj, nint sel, int int1);

    [LibraryImport(LibObjCLib, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool objc_msgSend_retBool(nint obj, nint sel, nint int1);

    [LibraryImport(LibObjCLib)]
    public static partial nint objc_allocateClassPair(nint superclass, nint name, int extrabytes);

    [LibraryImport(LibObjCLib)]
    public static partial void objc_registerClassPair(nint @class);

    [LibraryImport(LibObjCLib)]
    public static partial nint sel_registerName([MarshalAs(UnmanagedType.LPStr)] string cfstring);


    // Class builder
    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSelId_Bool fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool IdSelId_Bool(nint self, nint sel, nint v1);

    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSelIdId_Bool fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool IdSelIdId_Bool(nint self, nint sel, nint v1, nint v2);

    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSel_Void fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void IdSel_Void(nint self, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSel_Id fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate nint IdSel_Id(nint self, nint sel);

    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSelId_Void fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void IdSelId_Void(nint self, nint sel, nint v1);

    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSelIdId_Void fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void IdSelIdId_Void(nint self, nint sel, nint v1, nint v2);

    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSelNSRect_Void fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void IdSelNSRect_Void(nint self, nint sel, NSRect rect);

    [LibraryImport(LibObjCLib, EntryPoint = "class_addMethod")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static unsafe partial bool class_addMethod(nint objcclass, nint name, [MarshalAs(UnmanagedType.FunctionPtr)] IdSel_Bool fun, [MarshalAs(UnmanagedType.LPStr)] string types);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool IdSel_Bool(nint self, nint sel);
}