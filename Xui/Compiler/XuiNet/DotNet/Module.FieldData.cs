using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

public sealed partial class Module
{
    internal BlobReader FieldData(FieldDefinitionHandle handle, int length)
    {
        int rva = Metadata.GetFieldDefinition(handle).GetRelativeVirtualAddress();
        if (rva == 0) throw new InvalidOperationException("The field has no initial data in the PE.");
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        return assembly.PE.GetSectionData(rva).GetReader(0, length);
    }
}
