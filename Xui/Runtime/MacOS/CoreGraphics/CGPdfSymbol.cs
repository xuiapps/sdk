using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Symbols;

namespace Xui.Runtime.MacOS;

public static partial class CoreGraphics
{
    // PDF is Quartz's portable vector recording format. Unlike a bitmap or a CGLayer
    // optimized for a particular destination, it retains curves at arbitrary zoom.
    internal sealed partial class CGPdfSymbol : SafeHandle
    {
        internal CGPdfSymbol(SymbolContent content, Action<nint> draw) : base(0, true)
        {
            nint data = CFDataCreateMutable(0, 0), consumer = 0, context = 0, provider = 0;
            try
            {
                if (data == 0) throw new InvalidOperationException("Cannot allocate PDF data.");
                consumer = CGDataConsumerCreateWithCFData(data);
                var bounds = new CGRect(content.Bounds.X, content.Bounds.Y, content.Bounds.Width, content.Bounds.Height);
                context = CGPDFContextCreate(consumer, in bounds, 0);
                if (context == 0) throw new InvalidOperationException("Cannot create a symbol PDF context.");
                CGPDFContextBeginPage(context, 0);
                draw(context);
                CGPDFContextEndPage(context);
                CGPDFContextClose(context);
                provider = CGDataProviderCreateWithCFData(data);
                SetHandle(CGPDFDocumentCreateWithProvider(provider));
                if (IsInvalid) throw new InvalidOperationException("Cannot reopen the symbol PDF.");
            }
            finally
            {
                if (provider != 0) CGDataProviderRelease(provider);
                if (context != 0) CGContextRelease(context);
                if (consumer != 0) CGDataConsumerRelease(consumer);
                if (data != 0) CoreFoundation.CFRelease(data);
            }
        }
        public override bool IsInvalid => handle == 0;
        internal void Draw(nint context)
        {
            CGContextDrawPDFPage(context, CGPDFDocumentGetPage(handle, 1));
            GC.KeepAlive(this);
        }
        protected override bool ReleaseHandle() { CGPDFDocumentRelease(handle); handle=0; return true; }
        [LibraryImport(CoreFoundation.CoreFoundationLib)] private static partial nint CFDataCreateMutable(nint allocator, nint capacity);
        [LibraryImport(CoreGraphicsLib)] private static partial nint CGDataConsumerCreateWithCFData(nint data);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGDataConsumerRelease(nint consumer);
        [LibraryImport(CoreGraphicsLib)] private static partial nint CGPDFContextCreate(nint consumer, in CGRect bounds, nint info);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGPDFContextBeginPage(nint context, nint info);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGPDFContextEndPage(nint context);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGPDFContextClose(nint context);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGContextRelease(nint context);
        [LibraryImport(CoreGraphicsLib)] private static partial nint CGDataProviderCreateWithCFData(nint data);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGDataProviderRelease(nint provider);
        [LibraryImport(CoreGraphicsLib)] private static partial nint CGPDFDocumentCreateWithProvider(nint provider);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGPDFDocumentRelease(nint document);
        [LibraryImport(CoreGraphicsLib)] private static partial nint CGPDFDocumentGetPage(nint document, nuint page);
        [LibraryImport(CoreGraphicsLib)] private static partial void CGContextDrawPDFPage(nint context, nint page);
    }
}
