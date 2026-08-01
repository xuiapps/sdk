using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.IOS.CoreFoundation;
using static Xui.Runtime.IOS.CoreGraphics;
using static Xui.Runtime.IOS.Foundation;

namespace Xui.Runtime.IOS;

public partial class ObjC
{
    public class Builder
    {
        private string name;
        private nint self;

        public Builder(Class super, string name, nuint extraBytes = 0)
        {
            this.name = name;

            nint existingClass = objc_getClass(name);
            if (existingClass != 0)
            {
                var message =
                    $"Objective-C class allocation collision: cannot derive '{name}' from " +
                    $"'{super.Name}' because registered class '{class_getName(existingClass)}' already exists.";

                // Debug.WriteLine is not connected to the device debugger by default. More
                // importantly, objc_allocateClassPair can crash instead of returning nil for a
                // duplicate class on iOS, so never make the native call after this check.
                Console.Error.WriteLine(message);
                throw new ObjCException(message);
            }

            var nameAnsiCStrPtr = Marshal.StringToHGlobalAnsi(name);
#if DEBUG
            Console.Error.WriteLine($"Objective-C allocating class '{name}' derived from '{super.Name}'.");
#endif
            this.self = objc_allocateClassPair(super, nameAnsiCStrPtr, extraBytes);
            if (this.self == 0)
            {
                throw new ObjCException($"Objective-C allocating derived class '{name}' from class '{super.Name}' returned 0.");
            }
        }

        public Builder AddProtocol(Protocol protocol)
        {
            if (protocol.Id != 0)
            {
                // There "formal protocols" and "informal protocols", and informal protocols won't have Objective-C object.
                if (!class_addProtocol(this, protocol))
                {
                    throw new ObjCException($"Objective-C '{this.name}' class_addProtocol '{protocol.Name}' failed.");
                }
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSel_Bool method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "b@:@"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }
        public unsafe Builder AddMethod(string selector, ObjC.IdSelId_Void method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "v@:"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSelId_Bool method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "v@:"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSelIdId_Bool method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "B@:@@"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSel_Void method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "v@:@"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSel_Id method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "@@:@"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSelIdId_Void method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "v@:@@"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSelIdId_Id method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "@@:@@"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSelCGRect_Void method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            var typeEncoding = Marshal.SizeOf<NFloat>() == 32 ? "v@:{CGRect=ff}" : "v@:{CGRect=dd}";
            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, typeEncoding))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        // Explicit encodings are needed for UITextInput's mixed structs, offsets and objects.
        public Builder AddMethod(string selector, Delegate method, string encoding)
        {
            var root = GCHandle.Alloc(method);
            if (!AddMethodPointer(this, sel_registerName(selector), Marshal.GetFunctionPointerForDelegate(method), encoding))
            {
                root.Free();
                throw new ObjCException($"Cannot register {name}.{selector}.");
            }
            return this;
        }

        public Builder AddValueStorage(string name, nuint size)
        {
            if (!AddIvar(this, name, size, 3, $"[{size}C]"))
                throw new ObjCException($"Cannot register {this.name}.{name}.");
            return this;
        }

        [DllImport(LibObjCLib, EntryPoint = "class_addMethod")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AddMethodPointer(nint cls, nint selector, nint implementation, string encoding);

        [DllImport(LibObjCLib, EntryPoint = "class_addIvar")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AddIvar(nint cls, string name, nuint size, byte alignment, string encoding);

        public Class Register()
        {
#if DEBUG
            Console.Error.WriteLine($"Objective-C registering class '{this.name}'.");
#endif
            objc_registerClassPair(this.self);
            return new Class(this.name, this.self);
        }

        public static implicit operator nint(Builder cls) => cls.self;
    }
}
