using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Xui.Compiler.Playground;

[SupportedOSPlatform("browser")]
public static partial class Program
{
    public static void Main() { }

    [JSExport]
    public static byte[] Compile(string source) => PlaygroundCompiler.Compile(source);
}
