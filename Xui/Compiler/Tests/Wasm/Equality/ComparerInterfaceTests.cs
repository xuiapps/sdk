using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class ComparerInterfaceTests
{
    [Fact]
    public Task DefaultAndApplicationComparersDispatchIndependently() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public class Custom : IEqualityComparer<int> {
            public bool Equals(int left,int right) => left+1==right;
            public int GetHashCode(int value) => value+100;
        }
        public static class Program {
            static IEqualityComparer<int> Pick(bool custom) { if(custom)return new Custom(); return EqualityComparer<int>.Default; }
            static void Run(IEqualityComparer<int> comparer) {
                Console.WriteLine(comparer.GetHashCode(10)); Console.WriteLine(comparer.Equals(10,11));
            }
            public static int Main() {
                Run(Pick(false)); Run(Pick(true));
                try { Run(null); } catch(NullReferenceException) { Console.WriteLine("null"); }
                return 42;
            }
        }
        """, "10\nFalse\n110\nTrue\nnull\n", 42);

    [Fact]
    public Task DefaultSingletonCastsUseNominalIdentity() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public class Custom : IEqualityComparer<int> {
            public bool Equals(int left,int right) => left==right;
            public int GetHashCode(int value) => value;
        }
        public static class Program {
            public static int Main() {
                object integers=EqualityComparer<int>.Default, strings=EqualityComparer<string>.Default, custom=new Custom(), empty=null;
                Console.WriteLine(integers is IEqualityComparer<int>);
                Console.WriteLine(integers is IEqualityComparer<string>);
                Console.WriteLine(strings is IEqualityComparer<string>);
                Console.WriteLine(custom is IEqualityComparer<int>);
                Console.WriteLine(custom as EqualityComparer<int> == null);
                Console.WriteLine(((EqualityComparer<int>)integers).GetHashCode(42));
                Console.WriteLine(empty as IEqualityComparer<int> == null);
                try { _=(IEqualityComparer<int>)strings; } catch(InvalidCastException) { Console.WriteLine("cast"); }
                return 42;
            }
        }
        """, "True\nFalse\nTrue\nTrue\nTrue\n42\nTrue\ncast\n", 42);

    [Fact]
    public Task InterfaceEqualityKeepsStringAndFloatingPointSemantics() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public static class Program {
            static bool Text(IEqualityComparer<string> c,string a,string b)=>c.Equals(a,b);
            public static int Main() {
                IEqualityComparer<float> singles=EqualityComparer<float>.Default;
                IEqualityComparer<double> doubles=EqualityComparer<double>.Default;
                Console.WriteLine(singles.Equals(float.NaN,float.NaN));
                Console.WriteLine(singles.Equals(-0f,0f));
                Console.WriteLine(singles.Equals(float.NaN,0f));
                Console.WriteLine(doubles.Equals(double.NaN,double.NaN));
                Console.WriteLine(Text(EqualityComparer<string>.Default,null,null));
                Console.WriteLine(Text(EqualityComparer<string>.Default,"Ж😀","Ж😀"));
                Console.WriteLine(Text(EqualityComparer<string>.Default,null,""));
                return 42;
            }
        }
        """, "True\nTrue\nFalse\nTrue\nTrue\nTrue\nFalse\n", 42);
}
