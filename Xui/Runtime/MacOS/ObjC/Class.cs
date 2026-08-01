using System.Collections.Generic;

namespace Xui.Runtime.MacOS;

public static partial class ObjC
{
    /// <summary>
    /// Abstracts operations on Objective-C Class object.
    /// </summary>
    public class Class
    {
        public static readonly Sel AllocSel = new Sel("alloc");
        public static readonly Sel InitSel = new Sel("init");

        private bool init;
        private nint self;

        private List<object> classMethods = new List<object>();

        public Class(nint lib, string name)
        {
            this.Name = name;
            this.self = 0;
            this.init = false;
        }

        public Class(string name, nint id)
        {
            if (id == 0)
            {
                throw new ObjCException($"Objective-C Class '{this.Name}' constructed with 0.");
            }
            
            this.Name = name;
            this.self = id;
            this.init = true;
        }

        public string Name { get; }

        public nint Id
        {
            get
            {
                if (!this.init)
                {
                    this.init = true;
                    this.self = objc_getClass(this.Name);

                    if (this.self == 0)
                    {
                        throw new ObjCException($"Objective-C objc_getClass for '{this.Name}' returned 0.");
                    }
                }

                return this.self;
            }
        }

        public nint New()
        {
            nint id = this.Alloc();

            id = objc_msgSend_retIntPtr(id, InitSel);
            if (id == 0)
            {
                throw new ObjCException($"Objective-C {this.Name} init returned 0.");
            }

            return id;
        }

        public nint Alloc()
        {
            nint id = objc_msgSend_retIntPtr(this, AllocSel);
            if (id == 0)
            {
                throw new ObjCException($"Objective-C {this.Name} alloc returned 0.");
            }

            return id;
        }

        /// <summary>
        /// Creates a runtime derived class.
        /// Under the hood calls into objc_allocateClassPair
        /// https://developer.apple.com/documentation/objectivec/1418559-objc_allocateclasspair?language=objc
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Builder Extend(string name, nuint extraBytes = 0) => new Builder(this, name, extraBytes);

        public static implicit operator nint(Class cls) => cls.Id;
    }
}
