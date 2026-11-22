namespace Xui.Compiler.Tests.Wasm.Repl;

/// <summary>Captured console output and the main return value, with void mapped to exit code zero.</summary>
internal readonly record struct NodeResult(
    string Output,
    int ExitCode);
