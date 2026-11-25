namespace Xui.Compiler.WASM.Emitter;

/// <summary>Single-agent initialization state stored once per closed type and module instance.</summary>
internal enum TypeInitializationState
{
    Uninitialized,
    Initializing,
    Initialized,
    Failed
}
