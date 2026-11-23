using Xui.Compiler.WASM.Wasm.Indices;
using Writer = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Runtime;

/// <summary>Generates nominal interface membership checks and the class-parent walk for isinst.</summary>
internal static class InstanceTest
{
    public static void Emit(Writer writer, TypeIndex classRoot, TypeIndex descriptor, IReadOnlyCollection<Type> types)
    {
        LocalIndex value = new(0), target = new(1), current = new(2);
        writer.LocalGet(value).RefIsNull().If().RefNull(ValueTypeCode.EqRef).Return().End();
        writer.LocalGet(value).RefCast(classRoot).StructGet(classRoot, new(0)).LocalSet(current);
        // Membership is a closed-world relation independent of WASM structural equivalence.
        // Generate only interfaces retained by this module, before the ordinary class-parent walk.
        foreach (var contract in types.Where(type => type.IsInterface))
        {
            writer.LocalGet(target).GlobalGet(contract.Descriptor).RefEq().If();
            foreach (var implementation in types.Where(type => !type.IsInterface && type.Interfaces.Contains(contract)))
                writer.LocalGet(current).GlobalGet(implementation.Descriptor).RefEq().If().LocalGet(value).Return().End();
            writer.RefNull(ValueTypeCode.EqRef).Return().End();
        }
        writer.Loop();
        writer.LocalGet(current).LocalGet(target).RefEq().If().LocalGet(value).Return().End();
        writer.LocalGet(current).RefCast(descriptor).StructGet(descriptor, new((uint)DescriptorField.BaseType)).LocalTee(current)
            .RefIsNull().If().RefNull(ValueTypeCode.EqRef).Return().End();
        writer.Br(new(0)).End().Unreachable();
    }
}
