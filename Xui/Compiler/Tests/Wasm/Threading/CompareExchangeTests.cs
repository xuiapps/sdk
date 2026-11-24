using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

[Collection(nameof(ManagedConsoleCollection))]
public class CompareExchangeTests
{
    [Fact]
    public void RuntimeCompareExchangeObjectDefinitionIsRecognizedWithoutReadingItsBody()
    {
        using var framework = new Frontend.Framework();
        var module = framework.Assemblies[0].ManifestModule;
        var owner = module.Types.Single(h => module.GetType(h).Namespace == "System.Threading" && module.GetType(h).Name == "Interlocked");
        var handle = module.GetType(owner).Methods.Single(h => module.GetMethod(h).Name == "CompareExchangeObject");
        Assert.Null(module.GetMethod(handle).Body);
        var signature = Xui.Compiler.XuiNet.DotNet.Intrinsics.ReferenceCompareExchange.Read(module, handle, null);
        Assert.NotNull(signature);
        Assert.Equal(SignatureTypeCode.Object, signature.Value.Result.Code);
        Assert.Equal(signature.Value.Result.ByReference(), signature.Value.Parameters[0]);
    }

    [Fact]
    public Task ApplicationNamesakesAreNotTreatedAsFrameworkIntrinsics() => GcTests.AssertExecution("""
        namespace System.Threading {
            public static class Interlocked {
                public static object CompareExchange(ref object slot, object value, object expected) => value;
            }
        }
        public static class Program {
            public static int Main() {
                var first = new Item(); var next = new Item(); object slot = first;
                object result = System.Threading.Interlocked.CompareExchange(ref slot, next, first);
                return object.ReferenceEquals(result, next) && object.ReferenceEquals(slot, first) ? 42 : 0;
            }
        }
        public class Item { }
        """, "", 42);

    [Fact]
    public async Task DirectRuntimeEntryPointUsesTheSameCompareExchangeSemantics()
    {
        MemberReferenceHandle exchange = default, constructor = default;
        var pe = Frontend.Msil.Compile(il =>
        {
            // Exact-IL backend test of the private runtime entry point. Public overload tests provide CLR parity.
            il.OpCode(ILOpCode.Newobj); il.Token(constructor); il.StoreLocal(0);
            il.OpCode(ILOpCode.Newobj); il.Token(constructor); il.StoreLocal(1);
            il.LoadLocal(0); il.StoreLocal(2);
            il.LoadLocalAddress(2); il.LoadLocal(1); il.OpCode(ILOpCode.Ldnull);
            il.Call(exchange); il.StoreLocal(3);
            il.LoadLocal(3); il.LoadLocal(0); il.OpCode(ILOpCode.Ceq);
            il.LoadLocal(2); il.LoadLocal(0); il.OpCode(ILOpCode.Ceq); il.OpCode(ILOpCode.And);
            il.LoadLocalAddress(2); il.LoadLocal(1); il.LoadLocal(0); il.Call(exchange); il.StoreLocal(3);
            il.LoadLocal(3); il.LoadLocal(0); il.OpCode(ILOpCode.Ceq); il.OpCode(ILOpCode.And);
            il.LoadLocal(2); il.LoadLocal(1); il.OpCode(ILOpCode.Ceq); il.OpCode(ILOpCode.And);
            il.OpCode(ILOpCode.Ret);
        }, Enumerable.Repeat(SignatureTypeCode.Object, 4).ToArray(), configureMetadata: metadata =>
        {
            var owner = metadata.AddTypeReference(MetadataTokens.AssemblyReferenceHandle(1),
                metadata.GetOrAddString("System.Threading"), metadata.GetOrAddString("Interlocked"));
            var blob = new BlobBuilder();
            new BlobEncoder(blob).MethodSignature().Parameters(3, r => r.Type().Object(), p =>
            {
                p.AddParameter().Type(isByRef: true).Object(); p.AddParameter().Type().Object(); p.AddParameter().Type().Object();
            });
            exchange = metadata.AddMemberReference(owner, metadata.GetOrAddString("CompareExchangeObject"), metadata.GetOrAddBlob(blob));
            blob = new();
            new BlobEncoder(blob).MethodSignature(isInstanceMethod: true).Parameters(0, r => r.Void(), _ => { });
            constructor = metadata.AddMemberReference(MetadataTokens.TypeReferenceHandle(1),
                metadata.GetOrAddString(".ctor"), metadata.GetOrAddBlob(blob));
        });
        using var app = Xui.Compiler.Tests.Wasm.Canvas.CanvasTests.Read(pe);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var wasm = new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app);
        Assert.Equal(1, (await Xui.Compiler.Tests.Wasm.Repl.Node.ExecuteAsync(wasm)).ExitCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ClassFieldsAndFrameSlotsPreserveIdentityAndOriginalValue(bool linked)
    {
        const string source = """
            using System;
            using System.Threading;
            public class Item { public int Value; public override bool Equals(object other) => true; public override int GetHashCode() => 0; }
            public class Holder { public Item Value; }
            public static class Program {
                static T Exchange<T>(ref T field, T value, T expected) where T : class => Interlocked.CompareExchange(ref field, value, expected);
                static Item Argument(Item value, Item next) => Exchange(ref value, next, value);
                public static int Main() {
                    var a = new Item(); var b = new Item(); var c = new Item();
                    var holder = new Holder { Value = a };
                    Console.WriteLine(Exchange(ref holder.Value, b, c) == a);
                    Console.WriteLine(holder.Value == a);
                    Console.WriteLine(Exchange(ref holder.Value, b, a) == a);
                    Console.WriteLine(holder.Value == b);
                    Console.WriteLine(Exchange(ref holder.Value, null, b) == b);
                    Console.WriteLine(holder.Value == null);
                    Console.WriteLine(Exchange(ref holder.Value, a, null) == null);
                    Item local = a;
                    Console.WriteLine(Exchange(ref local, b, a) == a);
                    Console.WriteLine(local == b);
                    Console.WriteLine(Argument(a, b) == a);
                    object slot = a;
                    Console.WriteLine(Interlocked.CompareExchange(ref slot, b, c) == a);
                    Console.WriteLine(Interlocked.CompareExchange(ref slot, b, a) == a);
                    Console.WriteLine(slot == b);
                    return 42;
                }
            }
            """;
        string output = string.Concat(Enumerable.Repeat("True\n", 13));
        return linked ? InlineArrayTests.AssertLinked(source, output, 42) : GcTests.AssertExecution(source, output, 42);
    }

    [Fact]
    public Task SavedOwnerAddressAndReferenceStructFieldsRemainAliased() => GcTests.AssertExecution("""
        using System;
        using System.Threading;
        public class Item { }
        public class Holder { public Item Value; }
        public struct Event { public Item Value; }
        public static class Program {
            static Item Change(ref Event e, Item next, Item expected) => Interlocked.CompareExchange(ref e.Value, next, expected);
            public static int Main() {
                var a = new Item(); var b = new Item();
                var original = new Holder { Value = a }; var holder = original;
                ref Item saved = ref holder.Value;
                holder = new Holder();
                Console.WriteLine(Interlocked.CompareExchange(ref saved, b, a) == a);
                Console.WriteLine(original.Value == b);
                Console.WriteLine(holder.Value == null);
                Event e = new Event { Value = a };
                Console.WriteLine(Change(ref e, b, a) == a);
                Console.WriteLine(e.Value == b);
                int[] first = new int[1]; int[] second = new int[2]; var array = first;
                Console.WriteLine(Interlocked.CompareExchange(ref array, second, first) == first);
                Console.WriteLine(array == second);
                return 42;
            }
        }
        """, "True\nTrue\nTrue\nTrue\nTrue\nTrue\nTrue\n", 42);

    [Fact]
    public Task GeneratedEventAccessorsSubscribeUnsubscribeAndKeepInvocationSnapshot() => InlineArrayTests.AssertLinked("""
        using System;
        public struct Vector { public int X; public int Y; }
        public class Source {
            public event Action<Vector> Changed;
            public void Raise(int x, int y) { Changed?.Invoke(new Vector { X = x, Y = y }); }
        }
        public class Listener {
            public Source Source;
            public void A(Vector value) { Console.WriteLine(value.X); Source.Changed -= new Action<Vector>(B); }
            public void B(Vector value) { Console.WriteLine(value.Y); }
        }
        public static class Program {
            public static int Main() {
                var source = new Source(); var listener = new Listener { Source = source };
                source.Changed += new Action<Vector>(listener.A); source.Changed += new Action<Vector>(listener.B);
                source.Raise(2, 3); source.Raise(4, 5);
                source.Changed -= new Action<Vector>(listener.A); source.Raise(6, 7);
                return 42;
            }
        }
        """, "2\n3\n4\n", 42);

    [Fact]
    public Task EqualDelegatesStillCompareByReferenceIdentity() => GcTests.AssertExecution("""
        using System;
        using System.Threading;
        public delegate void Callback();
        public static class Program {
            static void A() { }
            static void B() { }
            public static int Main() {
                var first = new Callback(A); var equal = new Callback(A); var replacement = new Callback(B);
                var slot = first;
                Console.WriteLine(first == equal);
                Console.WriteLine(object.ReferenceEquals(Interlocked.CompareExchange(ref slot, replacement, equal), first));
                Console.WriteLine(object.ReferenceEquals(slot, first));
                Console.WriteLine(object.ReferenceEquals(Interlocked.CompareExchange(ref slot, replacement, first), first));
                Console.WriteLine(object.ReferenceEquals(slot, replacement));
                return 42;
            }
        }
        """, "True\nTrue\nTrue\nTrue\nTrue\n", 42);

    [Fact]
    public Task NullFieldOwnersThrowBeforeTheExchange() => GcTests.AssertExecution("""
        public class Holder { public object Value; }
        public static class Program {
            public static int Main() {
                Holder owner = null; int result = 1;
                try { System.Threading.Interlocked.CompareExchange(ref owner.Value, null, null); }
                catch (System.NullReferenceException) { result = 42; }
                return result;
            }
        }
        """, "", 42);

    [Fact]
    public void StringIdentityCannotBeSilentlyReplacedWithContentEquality()
    {
        var pe = Frontend.Roslyn.Compile("""
            public static class Program {
                public static void Main() { string value = "a"; System.Threading.Interlocked.CompareExchange(ref value, "b", "a"); }
            }
            """);
        Assert.Contains("managed string identity", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }
}
