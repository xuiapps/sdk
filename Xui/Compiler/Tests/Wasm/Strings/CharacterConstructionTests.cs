using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class CharacterConstructionTests
{
    [Fact]
    public Task CharacterArraysAreCopiedWithClrNullAndSliceSemantics()
    {
        const string source = """
            using System;
            public static class Program {
                static void Slice(char[] data,int start,int length) {
                    try { string result=new string(data,start,length); Console.WriteLine(result.Length);foreach(char c in result)Console.WriteLine((int)c); }
                    catch(ArgumentNullException e){Console.WriteLine("null");Console.WriteLine(e.ParamName);}
                    catch(ArgumentOutOfRangeException e){Console.WriteLine("range");Console.WriteLine(e.ParamName);}
                }
                public static int Main() {
                    char[] data={'A','\ud83d','\ude42','\0','Ж'};
                    string copy=new string(data);data[0]='B';Console.WriteLine((int)copy[0]);
                    Console.WriteLine(new string((char[])null).Length);Console.WriteLine(new string(new char[0]).Length);
                    Slice(data,1,2);Slice(data,2,1);Slice(data,5,0);
                    Slice(data,-1,0);Slice(data,0,-1);Slice(data,6,0);Slice(data,4,2);Slice(data,1,int.MaxValue);
                    Slice(null,0,0);Slice(null,-1,-1);Slice(data,-1,-1);
                    return 42;
                }
            }
            """;
        var expected=GcTests.RunManaged(Frontend.Roslyn.Compile(source));
        return InlineArrayTests.AssertLinked(source,expected.Output,expected.ExitCode);
    }
}
