using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Persistent fields and single-agent initialization state for one closed type in a module instance.</summary>
internal sealed class StaticType(TypeIdentity identity)
{
    public TypeIdentity Identity { get; } = identity;
    public GcShape Fields { get; } = new(identity);
    public MethodIdentity? Constructor { get; set; }
    public GlobalIndex Holder { get; set; }
    public GlobalIndex State { get; set; }
    public GlobalIndex Failure { get; set; }
    public FunctionIndex Ensure { get; set; }
    public string Name => TypeInitialization.Name(Identity);
    public string FailureMessage => $"The type initializer for '{Name}' threw an exception.";
}
