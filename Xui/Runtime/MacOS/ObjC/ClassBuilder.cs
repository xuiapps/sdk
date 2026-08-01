using System.Runtime.InteropServices;
using static Xui.Runtime.MacOS.CoreFoundation;
using static Xui.Runtime.MacOS.CoreGraphics;
using static Xui.Runtime.MacOS.Foundation;

namespace Xui.Runtime.MacOS;

public partial class ObjC
{
    public class Builder
    {
        private string name;
        private nint self;

        public Builder(Class super, string name, nuint extraBytes = 0)
        {
            this.name = name;
            var nameAnsiCStrPtr = Marshal.StringToHGlobalAnsi(name);
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

        public unsafe Builder AddMethod(string selector, ObjC.IdSelIdNUInt_NUInt method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            if (!class_addMethod(this, objCSelector, method, "Q@:@Q"))
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
            if (!class_addMethod(this, objCSelector, method, "@@:"))
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
            if (!class_addMethod(this, objCSelector, method, "v@:@"))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public unsafe Builder AddMethod(string selector, ObjC.IdSelNSRect_Void method)
        {
            nint objCSelector = sel_registerName(selector);
            if (objCSelector == 0)
            {
                throw new ObjCException($"Objective-C sel_registerName for '{selector}' returned 0.");
            }

            GCHandle.Alloc(method);
            var typeEncoding = Marshal.SizeOf<NFloat>() == 32 ? "v@:{NSRect=ff}" : "v@:{NSRect=dd}";
            if (!class_addMethod(this, objCSelector, method, typeEncoding))
            {
                throw new ObjCException($"Objective-C class_addMethod_retBool for '{selector}' returned 0.");
            }

            return this;
        }

        public Class Register()
        {
            objc_registerClassPair(this.self);
            return new Class(this.name, this.self);
        }

        public static implicit operator nint(Builder cls) => cls.self;
    }
}
