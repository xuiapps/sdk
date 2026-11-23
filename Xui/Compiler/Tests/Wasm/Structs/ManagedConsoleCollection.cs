using Xunit;

namespace Xui.Compiler.Tests.Wasm.Structs;

// The CLR reference run temporarily captures the process-wide Console.Out.
[CollectionDefinition(nameof(ManagedConsoleCollection), DisableParallelization = true)]
public sealed class ManagedConsoleCollection;
