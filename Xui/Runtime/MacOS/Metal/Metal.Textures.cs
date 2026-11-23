using System;
using System.Runtime.InteropServices;
using Xui.GPU.Shaders.Types;

namespace Xui.Runtime.MacOS;

public static partial class Metal
{
    internal static nint CreateSampledTextureDescriptor(int width, int height, int mipCount)
    {
        var desc = CreateTexture2DDescriptor(width, height);
        ObjC_MsgSendULong(desc, Sel_RegisterName("setPixelFormat:"), 70); // RGBA8Unorm
        ObjC_MsgSendULong(desc, Sel_RegisterName("setUsage:"), 1); // ShaderRead
        ObjC_MsgSendULong(desc, Sel_RegisterName("setMipmapLevelCount:"), (ulong)mipCount);
        return desc;
    }

    internal static void Texture_Upload(nint texture, nint bytes, int width, int height, int level) =>
        TextureReplaceRegion(texture, Sel_RegisterName("replaceRegion:mipmapLevel:withBytes:bytesPerRow:"),
            new MTLRegion { SizeX = (ulong)width, SizeY = (ulong)height, SizeZ = 1 }, (nuint)level, bytes, (nuint)(width * 4));

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void TextureReplaceRegion(nint texture, nint sel, MTLRegion region, nuint level, nint bytes, nuint bytesPerRow);

    internal static nint CreateSampler(nint device, Sampler sampler)
    {
        var cls = ObjC_GetClass("MTLSamplerDescriptor");
        var desc = ObjC_MsgSend(ObjC_MsgSend(cls, Sel_RegisterName("alloc")), Sel_RegisterName("init"));
        try
        {
            ObjC_MsgSendULong(desc, Sel_RegisterName("setMinFilter:"), (ulong)sampler.MinFilter);
            ObjC_MsgSendULong(desc, Sel_RegisterName("setMagFilter:"), (ulong)sampler.MagFilter);
            ObjC_MsgSendULong(desc, Sel_RegisterName("setMipFilter:"), 2); // linear mip interpolation
            ObjC_MsgSendULong(desc, Sel_RegisterName("setSAddressMode:"), Address(sampler.AddressU));
            ObjC_MsgSendULong(desc, Sel_RegisterName("setTAddressMode:"), Address(sampler.AddressV));
            return ObjC_MsgSendNInt(device, Sel_RegisterName("newSamplerStateWithDescriptor:"), desc);
        }
        finally { Release(desc); }
    }

    private static ulong Address(AddressMode mode) => mode switch
    {
        AddressMode.ClampToEdge => 0, AddressMode.Repeat => 2,
        AddressMode.MirroredRepeat => 3, AddressMode.ClampToBorder => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
    };

    internal static void ConfigurePremultipliedBlend(nint descriptor, bool enabled)
    {
        var array = ObjC_MsgSend(descriptor, Sel_RegisterName("colorAttachments"));
        var attachment = ObjC_MsgSendULong(array, Sel_RegisterName("objectAtIndexedSubscript:"), 0);
        ObjC_MsgSendULong(attachment, Sel_RegisterName("setBlendingEnabled:"), enabled ? 1UL : 0UL);
        ObjC_MsgSendULong(attachment, Sel_RegisterName("setSourceRGBBlendFactor:"), 1); // One
        ObjC_MsgSendULong(attachment, Sel_RegisterName("setSourceAlphaBlendFactor:"), 1);
        ObjC_MsgSendULong(attachment, Sel_RegisterName("setDestinationRGBBlendFactor:"), 5); // OneMinusSourceAlpha
        ObjC_MsgSendULong(attachment, Sel_RegisterName("setDestinationAlphaBlendFactor:"), 5);
    }

    internal static void Encoder_SetTexture(nint encoder, nint texture, nint sampler, nuint slot)
    {
        BindResource(encoder, Sel_RegisterName("setFragmentTexture:atIndex:"), texture, slot);
        BindResource(encoder, Sel_RegisterName("setFragmentSamplerState:atIndex:"), sampler, slot);
    }

    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void BindResource(nint encoder, nint sel, nint resource, nuint slot);

    internal static void Encoder_SetBytes(nint encoder, nint data, nuint length)
    {
        SetBytes(encoder, Sel_RegisterName("setVertexBytes:length:atIndex:"), data, length, 1);
        SetBytes(encoder, Sel_RegisterName("setFragmentBytes:length:atIndex:"), data, length, 1);
    }
    [LibraryImport(ObjCLib, EntryPoint = "objc_msgSend")]
    private static partial void SetBytes(nint encoder, nint sel, nint data, nuint length, nuint slot);
}
