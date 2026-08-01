using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Filter;

namespace Xui.Runtime.IOS;

public static partial class CoreGraphics
{
    public ref partial struct CGGradientRef
    {
        [LibraryImport(CoreGraphicsLib)]
        public static partial void CGGradientRelease(nint self);

        [LibraryImport(CoreGraphicsLib)]
        public static partial nint CGGradientCreateWithColorComponents(nint cgColorSpaceRef, ref NFloat componentsArrPtr, ref NFloat locationsArrPtr, nint count);

        public readonly nint Self;

        public CGGradientRef(nint self)
        {
            if (self == 0)
            {
                throw new ObjCException($"{nameof(CGColorRef)} instantiated with nil self.");
            }

            this.Self = self;
        }

        public CGGradientRef(ReadOnlySpan<GradientStop> gradient) : this(Map(gradient, default)) {}

        internal CGGradientRef(ReadOnlySpan<GradientStop> gradient, FilterState filter) : this(Map(gradient, filter)) {}

        private static nint Map(ReadOnlySpan<GradientStop> gradient, FilterState filter)
        {
            using var rgb = CGColorSpaceRef.CreateDeviceRGB();
            Span<NFloat> components = stackalloc NFloat[gradient.Length * 4];
            Span<NFloat> locations = stackalloc NFloat[gradient.Length];
            for(int i = 0; i < gradient.Length; i++)
            {
                var stop = gradient[i];
                var color = filter.Apply(stop.Color);
                var o = i * 4;
                components[o + 0] = color.Red;
                components[o + 1] = color.Green;
                components[o + 2] = color.Blue;
                components[o + 3] = color.Alpha;
                locations[i] = stop.Offset;
            }
            return CGGradientCreateWithColorComponents(
                rgb,
                ref MemoryMarshal.GetReference(components),
                ref MemoryMarshal.GetReference(locations),
                gradient.Length);
        }

        public void Dispose()
        {
            if (this.Self != 0)
            {
                CGGradientRelease(this.Self);
            }
        }

        public static implicit operator nint(CGGradientRef cfColorRef) => cfColorRef.Self;
    }
}