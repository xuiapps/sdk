using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

[Collection(nameof(ManagedConsoleCollection))]
public class NumericParsingTests
{
    [Fact]
    public Task InvariantDoubleAndHexParsingMatchesStringsAndBorrowedSpans()
    {
        const string source = """
            using System;
            using System.Globalization;
            public static class Program {
                static void Double(string text, NumberStyles style, bool span) {
                    try {
                        double value=span ? double.Parse(text.AsSpan(),style,CultureInfo.InvariantCulture) : double.Parse(text,style,CultureInfo.InvariantCulture);
                        if(double.IsNaN(value)) Console.WriteLine("nan"); else Console.WriteLine(BitConverter.DoubleToInt64Bits(value));
                    } catch(ArgumentNullException) {Console.WriteLine("null");} catch(FormatException) {Console.WriteLine("format");} catch(ArgumentException) {Console.WriteLine("style");}
                }
                static void UInt(string text, NumberStyles style, bool span) {
                    try {
                        uint value=span ? uint.Parse(text.AsSpan(),style,CultureInfo.InvariantCulture) : uint.Parse(text,style,CultureInfo.InvariantCulture);
                        Console.WriteLine(value.ToString(CultureInfo.InvariantCulture));
                    } catch(ArgumentNullException) {Console.WriteLine("null");} catch(OverflowException) {Console.WriteLine("overflow");} catch(FormatException) {Console.WriteLine("format");} catch(ArgumentException) {Console.WriteLine("style");}
                }
                public static int Main() {
                    foreach(string text in new[]{null,""," ","0","-0","+42",".125","1.","1.25e-12","1e309","-1e-999","5e-324","1.7976931348623157e308","1,234.5","1,,2,","1e","0xff","NaN","-NaN","Infinity","-Infinity","\u0085+infinity\u0085","1\0\0","1\0 ","1\u00a0","- 1","1\n","\t1","1\r\n"})
                        foreach(var style in new[]{NumberStyles.Float|NumberStyles.AllowThousands,NumberStyles.Float,NumberStyles.None,NumberStyles.HexNumber}) {
                            Double(text,style,false);Double(text,style,true);
                        }
                    foreach(string text in new[]{null,"","0","-0","-1","+42","4294967295","4294967296","  ffCC0099  ","ffffffff","100000000","ff\0","0xff","1111","2","1.0","1\n"})
                        foreach(var style in new[]{NumberStyles.Integer,NumberStyles.HexNumber,NumberStyles.BinaryNumber,NumberStyles.None,(NumberStyles)(-1)}) {
                            UInt(text,style,false);UInt(text,style,true);
                        }
                    Console.WriteLine(double.Parse("1.25",CultureInfo.InvariantCulture));
                    Console.WriteLine(double.Parse("__1.25__".AsSpan(2,4),CultureInfo.InvariantCulture));
                    try {double.Parse((string)null,CultureInfo.InvariantCulture);} catch(ArgumentNullException) {Console.WriteLine("null constant");}
                    return 42;
                }
            }
            """;
        var memory = MetadataReference.CreateFromFile(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Memory.dll"));
        var pe = Frontend.Roslyn.Compile(source, additionalReferences: [memory]);
        string expected = GcTests.RunManaged(pe).Output;
        return InlineArrayTests.AssertLinked(source, expected, 42, System.Reflection.Assembly.Load("System.Memory"));
    }
}
