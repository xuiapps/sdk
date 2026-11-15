using Xui.GPU.Shaders.Types;
using System.Runtime.InteropServices;

namespace Xui.Runtime.IOS;

/// <summary>
/// Thin P/Invoke wrappers for the Apple Metal framework.
/// Uses Objective-C runtime and Metal API for hardware-accelerated rendering.
/// </summary>
public static partial class Metal
{
    private const string ObjCLib = "/usr/lib/libobjc.A.dylib";
    private const string FoundationLib = "/System/Library/Frameworks/Foundation.framework/Foundation";
    private const string MetalLib = "/System/Library/Frameworks/Metal.framework/Metal";

    private static readonly ObjC.Sel PresentDrawableSel = new("presentDrawable:");

    internal static void CommandBuffer_PresentDrawable(nint commandBuffer, nint drawable) =>
        ObjC.objc_msgSend(commandBuffer, PresentDrawableSel, drawable);

    // ---- Objective-C runtime ----

    [LibraryImport(ObjCLib, EntryPoint = "objc_getClass",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint ObjC_GetClass(string name);

    [LibraryImport(ObjCLib, EntryPoint = "sel_registerName",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint Sel_RegisterName(string name);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    internal static partial nint ObjC_MsgSend(nint self, nint sel);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    internal static partial nint ObjC_MsgSendNInt(nint self, nint sel, nint arg0);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    internal static partial nint ObjC_MsgSendULong(nint self, nint sel, ulong arg0);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    internal static partial nint ObjC_MsgSendNIntNInt(nint self, nint sel, nint arg0, nint arg1);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    internal static partial nint ObjC_MsgSendNIntNIntNInt(nint self, nint sel, nint arg0, nint arg1, nint arg2);

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    internal static partial void ObjC_MsgSendDouble(nint self, nint sel, double arg0);

    // ---- Metal functions ----

    [LibraryImport(MetalLib, EntryPoint = "MTLCreateSystemDefaultDevice")]
    internal static partial nint MTLCreateSystemDefaultDevice();

    // ---- NSString helpers ----

    /// <summary>Creates an NSString from a UTF-8 C# string.</summary>
    internal static nint NSStringFromString(string s)
    {
        var cls = ObjC_GetClass("NSString");
        var sel = Sel_RegisterName("stringWithUTF8String:");
        var bytes = System.Text.Encoding.UTF8.GetBytes(s + "\0");
        unsafe
        {
            fixed (byte* ptr = bytes)
                return ObjC_MsgSendNInt(cls, sel, (nint)ptr);
        }
    }

    // ---- NSError ----

    /// <summary>Extracts a description string from an NSError object.</summary>
    internal static string GetErrorDescription(nint nsError)
    {
        if (nsError == 0) return "(no error)";
        var sel = Sel_RegisterName("localizedDescription");
        var descStr = ObjC_MsgSend(nsError, sel);
        return GetUtf8String(descStr);
    }

    /// <summary>Gets the UTF-8 string contents of an NSString.</summary>
    internal static string GetUtf8String(nint nsString)
    {
        if (nsString == 0) return string.Empty;
        var sel = Sel_RegisterName("UTF8String");
        unsafe
        {
            var ptr = (byte*)ObjC_MsgSend(nsString, sel);
            if (ptr == null) return string.Empty;
            return Marshal.PtrToStringUTF8((nint)ptr) ?? string.Empty;
        }
    }

    // ---- Metal device methods ----

    /// <summary>Calls [MTLDevice newLibraryWithSource:options:error:]</summary>
    internal static unsafe nint Device_NewLibraryWithSource(nint device, nint source, nint options, out nint error)
    {
        var sel = Sel_RegisterName("newLibraryWithSource:options:error:");
        nint* pError = stackalloc nint[1];
        *pError = 0;
        var result = DeviceNewLibraryWithSource(device, sel, source, options, (nint)pError);
        error = *pError;
        return result;
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static unsafe partial nint DeviceNewLibraryWithSource(nint self, nint sel, nint source, nint options, nint pError);

    /// <summary>Calls [MTLDevice newCommandQueue]</summary>
    internal static nint Device_NewCommandQueue(nint device)
    {
        var sel = Sel_RegisterName("newCommandQueue");
        return ObjC_MsgSend(device, sel);
    }

    /// <summary>Calls [MTLDevice newRenderPipelineStateWithDescriptor:error:]</summary>
    internal static unsafe nint Device_NewRenderPipelineState(nint device, nint descriptor, out nint error)
    {
        var sel = Sel_RegisterName("newRenderPipelineStateWithDescriptor:error:");
        nint* pError = stackalloc nint[1];
        *pError = 0;
        var result = DeviceNewRenderPipelineState(device, sel, descriptor, (nint)pError);
        error = *pError;
        return result;
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static unsafe partial nint DeviceNewRenderPipelineState(nint self, nint sel, nint descriptor, nint pError);

    /// <summary>Calls [MTLDevice newTextureWithDescriptor:]</summary>
    internal static nint Device_NewTexture(nint device, nint descriptor)
    {
        var sel = Sel_RegisterName("newTextureWithDescriptor:");
        return ObjC_MsgSendNInt(device, sel, descriptor);
    }

    /// <summary>Calls [MTLDevice newBufferWithBytes:length:options:]</summary>
    internal static nint Device_NewBufferWithBytes(nint device, nint bytes, nuint length, ulong options)
    {
        var sel = Sel_RegisterName("newBufferWithBytes:length:options:");
        return DeviceNewBufferWithBytes(device, sel, bytes, length, options);
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nint DeviceNewBufferWithBytes(nint self, nint sel, nint bytes, nuint length, ulong options);

    /// <summary>Calls [MTLDevice newBufferWithLength:options:]</summary>
    internal static nint Device_NewBufferWithLength(nint device, nuint length, ulong options)
    {
        var sel = Sel_RegisterName("newBufferWithLength:options:");
        return DeviceNewBufferWithLength(device, sel, length, options);
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial nint DeviceNewBufferWithLength(nint self, nint sel, nuint length, ulong options);

    // ---- MTLLibrary methods ----

    /// <summary>Calls [MTLLibrary newFunctionWithName:]</summary>
    internal static nint Library_NewFunctionWithName(nint library, string name)
    {
        var sel = Sel_RegisterName("newFunctionWithName:");
        var nameStr = NSStringFromString(name);
        return ObjC_MsgSendNInt(library, sel, nameStr);
    }

    // ---- MTLRenderPipelineDescriptor ----

    /// <summary>Describes a single vertex attribute for MTLVertexDescriptor.</summary>
    internal struct VertexAttributeDesc
    {
        /// <summary>MTLVertexFormat enum value (e.g. Float2=29, Float3=30, Float4=31).</summary>
        public ulong Format;
        /// <summary>Byte offset of this attribute within the vertex struct.</summary>
        public ulong Offset;
        /// <summary>Which vertex buffer binding this attribute reads from.</summary>
        public ulong BufferIndex;
    }

    /// <summary>
    /// Creates an MTLVertexDescriptor that tells the hardware vertex fetch unit how to
    /// read tightly-packed C# vertex data and deliver GPU-native aligned types to the shader.
    /// </summary>
    internal static nint CreateVertexDescriptor(VertexAttributeDesc[] attributes, ulong stride)
    {
        var cls = ObjC_GetClass("MTLVertexDescriptor");
        var desc = ObjC_MsgSend(ObjC_MsgSend(cls, Sel_RegisterName("alloc")), Sel_RegisterName("init"));

        // Configure each vertex attribute
        var attrsArray = ObjC_MsgSend(desc, Sel_RegisterName("attributes"));
        var objectAtIndex = Sel_RegisterName("objectAtIndexedSubscript:");
        var setFormat = Sel_RegisterName("setFormat:");
        var setOffset = Sel_RegisterName("setOffset:");
        var setBufferIndex = Sel_RegisterName("setBufferIndex:");

        for (int i = 0; i < attributes.Length; i++)
        {
            var attr = ObjC_MsgSendULong(attrsArray, objectAtIndex, (ulong)i);
            ObjC_MsgSendULong(attr, setFormat, attributes[i].Format);
            ObjC_MsgSendULong(attr, setOffset, attributes[i].Offset);
            ObjC_MsgSendULong(attr, setBufferIndex, attributes[i].BufferIndex);
        }

        // Configure buffer layout 0: stride and per-vertex stepping
        var layoutsArray = ObjC_MsgSend(desc, Sel_RegisterName("layouts"));
        var layout0 = ObjC_MsgSendULong(layoutsArray, objectAtIndex, 0);
        ObjC_MsgSendULong(layout0, Sel_RegisterName("setStride:"), stride);
        ObjC_MsgSendULong(layout0, Sel_RegisterName("setStepFunction:"), 1); // MTLVertexStepFunctionPerVertex
        ObjC_MsgSendULong(layout0, Sel_RegisterName("setStepRate:"), 1);

        return desc;
    }

    /// <summary>Creates and configures an MTLRenderPipelineDescriptor.</summary>
    internal static nint CreateRenderPipelineDescriptor(nint vertexFunction, nint fragmentFunction, nint vertexDescriptor = 0, ulong depthPixelFormat = 0)
    {
        var cls = ObjC_GetClass("MTLRenderPipelineDescriptor");
        var allocSel = Sel_RegisterName("alloc");
        var initSel = Sel_RegisterName("init");
        var desc = ObjC_MsgSend(ObjC_MsgSend(cls, allocSel), initSel);

        // Set vertex function
        var setVS = Sel_RegisterName("setVertexFunction:");
        ObjC_MsgSendNInt(desc, setVS, vertexFunction);

        // Set fragment function
        var setFS = Sel_RegisterName("setFragmentFunction:");
        ObjC_MsgSendNInt(desc, setFS, fragmentFunction);

        // Set vertex descriptor (tells Metal how to fetch vertex attributes)
        if (vertexDescriptor != 0)
        {
            var setVertexDesc = Sel_RegisterName("setVertexDescriptor:");
            ObjC_MsgSendNInt(desc, setVertexDesc, vertexDescriptor);
        }

        // Set pixel format on colorAttachments[0] to BGRA8Unorm (80)
        var colAttachments = Sel_RegisterName("colorAttachments");
        var attArray = ObjC_MsgSend(desc, colAttachments);
        var objectAtIndex = Sel_RegisterName("objectAtIndexedSubscript:");
        var att0 = ObjC_MsgSendULong(attArray, objectAtIndex, 0);
        var setPixelFormat = Sel_RegisterName("setPixelFormat:");
        ObjC_MsgSendULong(att0, setPixelFormat, 80); // MTLPixelFormatBGRA8Unorm = 80

        // Set depth attachment pixel format if specified
        if (depthPixelFormat != 0)
        {
            ObjC_MsgSendULong(desc, Sel_RegisterName("setDepthAttachmentPixelFormat:"), depthPixelFormat);
        }

        return desc;
    }

    // ---- MTLTextureDescriptor ----

    /// <summary>Creates an MTLTextureDescriptor configured as a 2D render target.</summary>
    internal static nint CreateTexture2DDescriptor(int width, int height)
    {
        var cls = ObjC_GetClass("MTLTextureDescriptor");
        var allocSel = Sel_RegisterName("alloc");
        var initSel = Sel_RegisterName("init");
        var desc = ObjC_MsgSend(ObjC_MsgSend(cls, allocSel), initSel);

        // textureType = MTLTextureType2D (2)
        var setType = Sel_RegisterName("setTextureType:");
        ObjC_MsgSendULong(desc, setType, 2);

        // pixelFormat = MTLPixelFormatBGRA8Unorm (80)
        var setFormat = Sel_RegisterName("setPixelFormat:");
        ObjC_MsgSendULong(desc, setFormat, 80);

        // width and height
        var setWidth = Sel_RegisterName("setWidth:");
        ObjC_MsgSendULong(desc, setWidth, (ulong)width);
        var setHeight = Sel_RegisterName("setHeight:");
        ObjC_MsgSendULong(desc, setHeight, (ulong)height);

        // usage = MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead (0x1 | 0x4 = 0x5)
        var setUsage = Sel_RegisterName("setUsage:");
        ObjC_MsgSendULong(desc, setUsage, 0x5);

        // storageMode = MTLStorageModePrivate on iOS (2), Managed on macOS (1)
        // Use Shared (0) to allow CPU access for readback
        var setStorage = Sel_RegisterName("setStorageMode:");
        ObjC_MsgSendULong(desc, setStorage, 0); // MTLStorageModeShared = 0

        return desc;
    }

    // ---- MTLCommandQueue methods ----

    /// <summary>Calls [MTLCommandQueue commandBuffer]</summary>
    internal static nint CommandQueue_CommandBuffer(nint queue)
    {
        var sel = Sel_RegisterName("commandBuffer");
        return ObjC_MsgSend(queue, sel);
    }

    // ---- MTLCommandBuffer methods ----

    /// <summary>Calls [MTLCommandBuffer renderCommandEncoderWithDescriptor:]</summary>
    internal static nint CommandBuffer_RenderCommandEncoderWithDescriptor(nint buffer, nint descriptor)
    {
        var sel = Sel_RegisterName("renderCommandEncoderWithDescriptor:");
        return ObjC_MsgSendNInt(buffer, sel, descriptor);
    }

    /// <summary>Calls [MTLCommandBuffer commit]</summary>
    internal static void CommandBuffer_Commit(nint buffer)
    {
        var sel = Sel_RegisterName("commit");
        ObjC_MsgSend(buffer, sel);
    }

    /// <summary>Calls [MTLCommandBuffer waitUntilCompleted]</summary>
    internal static void CommandBuffer_WaitUntilCompleted(nint buffer)
    {
        var sel = Sel_RegisterName("waitUntilCompleted");
        ObjC_MsgSend(buffer, sel);
    }

    // ---- MTLRenderPassDescriptor ----

    /// <summary>Creates an MTLRenderPassDescriptor for a render target texture.</summary>
    internal static nint CreateRenderPassDescriptor(nint texture, Color4 clearColor, nint depthTexture = 0)
    {
        var cls = ObjC_GetClass("MTLRenderPassDescriptor");
        var allocSel = Sel_RegisterName("alloc");
        var initSel = Sel_RegisterName("init");
        var desc = ObjC_MsgSend(ObjC_MsgSend(cls, allocSel), initSel);

        var colAttachments = Sel_RegisterName("colorAttachments");
        var attArray = ObjC_MsgSend(desc, colAttachments);
        var objectAtIndex = Sel_RegisterName("objectAtIndexedSubscript:");
        var att0 = ObjC_MsgSendULong(attArray, objectAtIndex, 0);

        // Set texture
        var setTexture = Sel_RegisterName("setTexture:");
        ObjC_MsgSendNInt(att0, setTexture, texture);

        // loadAction = MTLLoadActionClear (2)
        var setLoad = Sel_RegisterName("setLoadAction:");
        ObjC_MsgSendULong(att0, setLoad, 2);

        // storeAction = MTLStoreActionStore (1)
        var setStore = Sel_RegisterName("setStoreAction:");
        ObjC_MsgSendULong(att0, setStore, 1);

        // clearColor = MTLClearColor struct (4 doubles: r, g, b, a)
        SetClearColor(att0, clearColor.R, clearColor.G, clearColor.B, clearColor.A);

        // Configure depth attachment if depth texture provided
        if (depthTexture != 0)
        {
            var depthAttachment = ObjC_MsgSend(desc, Sel_RegisterName("depthAttachment"));
            ObjC_MsgSendNInt(depthAttachment, setTexture, depthTexture);
            ObjC_MsgSendULong(depthAttachment, setLoad, 2);   // MTLLoadActionClear
            ObjC_MsgSendULong(depthAttachment, setStore, 0);  // MTLStoreActionDontCare
            ObjC_MsgSendDouble(depthAttachment, Sel_RegisterName("setClearDepth:"), 1.0);
        }

        return desc;
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SetClearColorNative(nint self, nint sel, double r, double g, double b, double a);

    private static void SetClearColor(nint attachment, float r, float g, float b, float a)
    {
        var sel = Sel_RegisterName("setClearColor:");
        SetClearColorNative(attachment, sel, r, g, b, a);
    }

    // ---- MTLRenderCommandEncoder methods ----

    /// <summary>Calls [encoder setRenderPipelineState:]</summary>
    internal static void Encoder_SetRenderPipelineState(nint encoder, nint pipelineState)
    {
        var sel = Sel_RegisterName("setRenderPipelineState:");
        ObjC_MsgSendNInt(encoder, sel, pipelineState);
    }

    /// <summary>Calls [encoder setVertexBuffer:offset:atIndex:]</summary>
    internal static void Encoder_SetVertexBuffer(nint encoder, nint buffer, nuint offset, ulong index)
    {
        var sel = Sel_RegisterName("setVertexBuffer:offset:atIndex:");
        EncoderSetVertexBuffer(encoder, sel, buffer, offset, index);
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void EncoderSetVertexBuffer(nint self, nint sel, nint buffer, nuint offset, ulong index);

    /// <summary>Calls [encoder setVertexBytes:length:atIndex:]</summary>
    internal static void Encoder_SetVertexBytes(nint encoder, nint bytes, nuint length, ulong index)
    {
        var sel = Sel_RegisterName("setVertexBytes:length:atIndex:");
        EncoderSetVertexBytes(encoder, sel, bytes, length, index);
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void EncoderSetVertexBytes(nint self, nint sel, nint bytes, nuint length, ulong index);

    /// <summary>Calls [encoder drawPrimitives:vertexStart:vertexCount:]</summary>
    internal static void Encoder_DrawPrimitives(nint encoder, ulong primitiveType, ulong vertexStart, ulong vertexCount)
    {
        var sel = Sel_RegisterName("drawPrimitives:vertexStart:vertexCount:");
        EncoderDrawPrimitives(encoder, sel, primitiveType, vertexStart, vertexCount);
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void EncoderDrawPrimitives(nint self, nint sel, ulong primitiveType, ulong vertexStart, ulong vertexCount);

    /// <summary>Calls [encoder endEncoding]</summary>
    internal static void Encoder_EndEncoding(nint encoder)
    {
        var sel = Sel_RegisterName("endEncoding");
        ObjC_MsgSend(encoder, sel);
    }

    // ---- MTLTexture readback ----

    /// <summary>Calls [texture getBytes:bytesPerRow:fromRegion:mipmapLevel:]</summary>
    internal static void Texture_GetBytes(nint texture, nint bytes, nuint bytesPerRow, MTLRegion region, ulong mipmapLevel)
    {
        var sel = Sel_RegisterName("getBytes:bytesPerRow:fromRegion:mipmapLevel:");
        TextureGetBytes(texture, sel, bytes, bytesPerRow, region, mipmapLevel);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MTLRegion
    {
        public ulong OriginX, OriginY, OriginZ;
        public ulong SizeX, SizeY, SizeZ;
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void TextureGetBytes(nint self, nint sel, nint bytes, nuint bytesPerRow, MTLRegion region, ulong mipmapLevel);

    // ---- Depth Stencil ----

    /// <summary>Creates an MTLDepthStencilDescriptor with the specified depth test/write settings.</summary>
    internal static nint CreateDepthStencilDescriptor(bool depthTestEnabled, bool depthWriteEnabled)
    {
        var cls = ObjC_GetClass("MTLDepthStencilDescriptor");
        var desc = ObjC_MsgSend(ObjC_MsgSend(cls, Sel_RegisterName("alloc")), Sel_RegisterName("init"));

        // setDepthCompareFunction: MTLCompareFunctionLess=1, MTLCompareFunctionAlways=7
        ulong compareFunc = depthTestEnabled ? 1UL : 7UL;
        ObjC_MsgSendULong(desc, Sel_RegisterName("setDepthCompareFunction:"), compareFunc);

        // setDepthWriteEnabled: pass 1 or 0 as ULong
        ObjC_MsgSendULong(desc, Sel_RegisterName("setDepthWriteEnabled:"), depthWriteEnabled ? 1UL : 0UL);

        return desc;
    }

    /// <summary>Calls [device newDepthStencilStateWithDescriptor:]</summary>
    internal static nint Device_NewDepthStencilState(nint device, nint descriptor)
    {
        var sel = Sel_RegisterName("newDepthStencilStateWithDescriptor:");
        return ObjC_MsgSendNInt(device, sel, descriptor);
    }

    /// <summary>Calls [encoder setDepthStencilState:]</summary>
    internal static void Encoder_SetDepthStencilState(nint encoder, nint state)
    {
        var sel = Sel_RegisterName("setDepthStencilState:");
        ObjC_MsgSendNInt(encoder, sel, state);
    }

    /// <summary>Calls [encoder setCullMode:] (MTLCullModeNone=0, MTLCullModeFront=1, MTLCullModeBack=2)</summary>
    internal static void Encoder_SetCullMode(nint encoder, ulong cullMode)
    {
        var sel = Sel_RegisterName("setCullMode:");
        ObjC_MsgSendULong(encoder, sel, cullMode);
    }

    /// <summary>Calls [encoder setFrontFacingWinding:] (MTLWindingClockwise=0, MTLWindingCounterClockwise=1)</summary>
    internal static void Encoder_SetFrontFacingWinding(nint encoder, ulong winding)
    {
        var sel = Sel_RegisterName("setFrontFacingWinding:");
        ObjC_MsgSendULong(encoder, sel, winding);
    }

    // ---- Depth Texture ----

    /// <summary>Creates a depth texture with MTLPixelFormatDepth32Float.</summary>
    internal static nint CreateDepthTexture(nint device, int width, int height)
    {
        var cls = ObjC_GetClass("MTLTextureDescriptor");
        var desc = ObjC_MsgSend(ObjC_MsgSend(cls, Sel_RegisterName("alloc")), Sel_RegisterName("init"));

        // textureType = MTLTextureType2D (2)
        ObjC_MsgSendULong(desc, Sel_RegisterName("setTextureType:"), 2);
        // pixelFormat = MTLPixelFormatDepth32Float (252)
        ObjC_MsgSendULong(desc, Sel_RegisterName("setPixelFormat:"), 252);
        // width / height
        ObjC_MsgSendULong(desc, Sel_RegisterName("setWidth:"), (ulong)width);
        ObjC_MsgSendULong(desc, Sel_RegisterName("setHeight:"), (ulong)height);
        // usage = MTLTextureUsageRenderTarget (4)
        ObjC_MsgSendULong(desc, Sel_RegisterName("setUsage:"), 4);
        // storageMode = MTLStorageModePrivate (2)
        ObjC_MsgSendULong(desc, Sel_RegisterName("setStorageMode:"), 2);

        var texture = Device_NewTexture(device, desc);
        Release(desc);
        return texture;
    }

    // ---- NSObject ----

    /// <summary>Calls [obj retain] and returns the object.</summary>
    internal static nint Retain(nint obj)
    {
        if (obj == 0) return 0;
        var sel = Sel_RegisterName("retain");
        return ObjC_MsgSend(obj, sel);
    }

    /// <summary>Calls [obj release]</summary>
    internal static void Release(nint obj)
    {
        if (obj == 0) return;
        var sel = Sel_RegisterName("release");
        ObjC_MsgSend(obj, sel);
    }
}
