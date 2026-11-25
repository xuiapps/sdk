namespace Xui.Compiler.WASM.Emitter;

// ABI shared with the scalar switch in Runtime/xui-runtime.js.
internal enum FormattingValueKind
{
    Unsupported,
    Boolean,
    Char,
    Signed32,
    Unsigned32,
    Signed64,
    Unsigned64,
    Single,
    Double,
    Signed8,
    Signed16,
    Unsigned8,
    Unsigned16,
    Text
}
