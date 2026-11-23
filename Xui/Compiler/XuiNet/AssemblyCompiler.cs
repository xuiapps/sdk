using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using IR = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet;

/// <summary>Opens a managed PE for borrowed metadata views and deferred method-body decoding.</summary>
public static class AssemblyCompiler
{
    /// <summary>Buffers the PE image without copying its declarations or metadata blobs, leaving the input stream open.</summary>
    /// <returns>An assembly that owns the PE reader and must be disposed after all borrowed views have been used.</returns>
    public static IR.Assembly Read(Stream input)
    {
        var pe = new PEReader(input, PEStreamOptions.PrefetchEntireImage | PEStreamOptions.LeaveOpen);
        try
        {
            var metadata = pe.GetMetadataReader();
            _ = metadata.GetAssemblyDefinition();
            if ((pe.PEHeaders.CorHeader!.Flags & CorFlags.NativeEntryPoint) != 0)
                throw new NotSupportedException("Native entry points are not supported.");
            return new IR.Assembly(pe, metadata);
        }
        catch
        {
            pe.Dispose();
            throw;
        }
    }
}
