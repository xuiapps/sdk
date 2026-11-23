using Xui.Compiler.WASM.Wasm.Indices;
using Writer = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Generates the shared isinst helper by following nominal descriptor parent links.</summary>
internal static class InstanceTest
{
    public static void Emit(Writer writer, TypeIndex classRoot, TypeIndex descriptor)
    {
        LocalIndex value = new(0), target = new(1), current = new(2);
        writer.LocalGet(value).RefIsNull().If().RefNull(ValueTypeCode.EqRef).Return().End();
        writer.LocalGet(value).RefCast(classRoot).StructGet(classRoot, new(0)).LocalSet(current);
        writer.Loop();
        writer.LocalGet(current).LocalGet(target).RefEq().If().LocalGet(value).Return().End();
        writer.LocalGet(current).RefCast(descriptor).StructGet(descriptor, new((uint)DescriptorField.BaseType)).LocalTee(current)
            .RefIsNull().If().RefNull(ValueTypeCode.EqRef).Return().End();
        writer.Br(new(0)).End().Unreachable();
    }
}
