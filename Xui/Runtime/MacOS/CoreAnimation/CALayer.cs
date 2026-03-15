using System;
using static Xui.Runtime.MacOS.ObjC;

namespace Xui.Runtime.MacOS;

public static partial class CoreAnimation
{
    public class CALayer : NSObject
    {
        public static new readonly Class Class = new Class(CoreAnimation.Lib, "CALayer");

        private static readonly Sel InsertSublayerAtIndexSel = new Sel("insertSublayer:atIndex:");

        private static readonly Prop.NInt MaskProp = new Prop.NInt("mask", "setMask:");

        public CALayer() : base(Class.New())
        {
        }

        public CALayer(nint id) : base(id)
        {
        }

        public CALayer Mask
        {
            get => throw new NotImplementedException("Not handled nint to CALayer marshalling.");
            set => MaskProp.Set(this, value);
        }

        public void InsertAt(CALayer layer, uint index) => objc_msgSend(this, InsertSublayerAtIndexSel, layer, index);
    }
}
