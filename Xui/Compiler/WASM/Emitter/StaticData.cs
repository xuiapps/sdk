using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet;
using DataWriter = Xui.Compiler.WASM.Wasm.Section.Data.Writer;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Retained immutable PE blobs placed after the bounded linear frame stack, once per module instance.</summary>
internal sealed class StaticData
{
    private readonly Dictionary<FieldIdentity, (int Address, int Length)> fields = new();
    private int end = MemoryFrame.Capacity;
    public uint Count => (uint)fields.Count;
    public uint MemoryPages => (uint)(((long)end + 65535) / 65536);

    public void Add(StaticSpanCall span)
    {
        if (fields.ContainsKey(span.Field)) return;
        int address = checked((end + 7) & ~7);
        end = checked(address + span.ByteLength);
        fields.Add(span.Field, (address, span.ByteLength));
    }

    public int Address(FieldIdentity field) => fields[field].Address;

    public void Write(ref DataWriter writer)
    {
        foreach (var (field, data) in fields)
        {
            var reader = field.DeclaringType.Module.GetField(field.Handle).InitialData(data.Length);
            writer.Active(data.Address, reader.ReadBytes(data.Length));
        }
    }
}
