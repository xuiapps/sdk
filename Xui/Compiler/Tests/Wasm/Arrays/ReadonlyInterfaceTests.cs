using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class ReadonlyInterfaceTests
{
    [Fact]
    public Task ArraysAndClassesShareReadonlyInterfaceCalls() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public static class Program {
            static int Sum(IReadOnlyList<int> values) { int sum=0; for(int i=0;i<values.Count;i++) sum+=values[i]; return sum; }
            public static int Main() {
                int[] array={10,12,20}; var list=new List<int>(); list.Add(40); list.Add(2);
                Console.WriteLine(Sum(array)); Console.WriteLine(Sum(list));
                object other = new uint[1]; object boxed=array;
                Console.WriteLine(boxed is IReadOnlyList<int>); Console.WriteLine(boxed is IReadOnlyList<uint>);
                Console.WriteLine(other is IReadOnlyList<uint>); Console.WriteLine(other is IReadOnlyList<int>);
                Console.WriteLine(((IReadOnlyCollection<int>)boxed).Count);
                IReadOnlyList<int> values=array;
                try { Console.WriteLine(values[-1]); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                return Sum(values);
            }
        }
        """, "42\n42\nTrue\nTrue\nTrue\nTrue\n3\nindex\n", 42, System.Reflection.Assembly.Load("System.Collections"));

    [Fact]
    public Task ReferenceArrayCastsKeepManagedElementIdentity() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        class A {} class B : A {} class C {}
        public static class Program {
            public static int Main() {
                object a=new A[1]; object b=new B[1]; object c=new C[1];
                Console.WriteLine(a is IReadOnlyList<A>); Console.WriteLine(a is IReadOnlyList<B>);
                Console.WriteLine(b is IReadOnlyList<B>); Console.WriteLine(b is IReadOnlyList<A>);
                Console.WriteLine(c is IReadOnlyList<A>);
                try { var wrong=(IReadOnlyList<B>)a; } catch(InvalidCastException) { Console.WriteLine("cast"); }
                return 42;
            }
        }
        """, "True\nFalse\nTrue\nTrue\nFalse\ncast\n", 42);

    [Fact]
    public Task StructItemsAreCopiedThroughReadonlyInterface() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        struct Point { public int X,Y; }
        public static class Program {
            static Point Read(IReadOnlyList<Point> values) => values[0];
            static (string,int) ReadText(IReadOnlyList<(string,int)> values) => values[0];
            public static int Main() {
                var points=new Point[1]; points[0]=new Point {X=20,Y=22};
                var copy=Read(points); copy.X=99;
                var texts=new (string,int)[1]; texts[0]=("hello",42);
                var text=ReadText(texts); text.Item1="changed";
                Console.WriteLine(ReadText(texts).Item1);
                return Read(points).X + Read(points).Y;
            }
        }
        """, "hello\n", 42);
}
