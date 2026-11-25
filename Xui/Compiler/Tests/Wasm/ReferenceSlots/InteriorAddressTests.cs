using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.ReferenceSlots;

[Collection(nameof(ManagedConsoleCollection))]
public class InteriorAddressTests
{
    [Fact]
    public async Task InitializedByrefLocalsStartAtTheNullLocation()
    {
        byte[] image = Frontend.Msil.Compile(il =>
        {
            var start = il.DefineLabel(); var handler = il.DefineLabel(); var end = il.DefineLabel();
            il.MarkLabel(start); il.LoadLocal(0); il.OpCode(ILOpCode.Ldind_i4); il.StoreLocal(1); il.Branch(ILOpCode.Leave, end);
            il.MarkLabel(handler); il.OpCode(ILOpCode.Pop); il.LoadConstantI4(42); il.StoreLocal(1); il.Branch(ILOpCode.Leave, end);
            il.MarkLabel(end); il.LoadLocal(1); il.OpCode(ILOpCode.Ret);
            il.ControlFlowBuilder!.AddCatchRegion(start, handler, handler, end, MetadataTokens.TypeReferenceHandle(1));
        }, rawLocalSignature: [(byte)SignatureKind.LocalVariables, 2, (byte)SignatureTypeCode.ByReference, (byte)SignatureTypeCode.Int32, (byte)SignatureTypeCode.Int32]);
        Assert.Equal(42, GcTests.RunManaged(image).ExitCode);
        Assert.Equal(42, (await Node.ExecuteAsync(GcTests.Emit(image))).ExitCode);
    }

    [Fact]
    public Task OneMethodMutatesLinearAndGcStructLocationsWithoutCopying() => GcTests.AssertExecution("""
        public class View { public int Value; }
        public struct Point { public int X; public int Y; }
        public struct Event { public string Text; public View Target; public Point Position; }
        public class Holder { public int Padding = 99; public Event Event; }
        public static class Program {
            static ref T Select<T>(bool first, ref T a, ref T b) => ref (first ? ref a : ref b);
            static void Change(ref Event value, int number) {
                value.Text = "changed"; value.Position.X = number; value.Target = new View { Value = number + 1 };
            }
            public static int Main() {
                var holder = new Holder(); Event local = default;
                ref Event alias = ref Select(false, ref local, ref holder.Event);
                Change(ref alias, 7); Change(ref Select(true, ref local, ref holder.Event), 3);
                Event copy = alias; copy.Position.X = 20; copy.Target.Value = 9;
                System.Console.WriteLine(holder.Event.Text);
                System.Console.WriteLine(local.Text);
                return holder.Padding + holder.Event.Position.X + local.Position.X + copy.Position.X + holder.Event.Target.Value;
            }
        }
        """, "changed\nchanged\n", 138);

    [Fact]
    public Task EntryValueReferencesSurviveAssignmentAndKeepTheOriginalArrayAlive() => GcTests.AssertExecution("""
        public struct Point { public int X; public int Y; }
        public struct Value { public string Name; public Point Position; }
        public struct Entry { public int Hash; public Value Value; public int Next; }
        public static class Program {
            static ref Value Find(Entry[] entries, int index) => ref entries[index].Value;
            public static int Main() {
                var entries = new Entry[2]; entries[1].Hash = 100;
                ref Value saved = ref Find(entries, 1);
                saved.Name = "old"; saved.Position.X = 4;
                entries[1].Value = new Value { Name = "replaced", Position = new Point { X = 8, Y = 2 } };
                System.Console.WriteLine(saved.Name);
                var grown = new Entry[4]; System.Array.Copy(entries, grown, 2); entries = grown;
                saved.Name = "original"; saved.Position.X = 20;
                System.Console.WriteLine(entries[1].Value.Name);
                System.Console.WriteLine(saved.Name);
                return entries[1].Hash + entries[1].Value.Position.X + saved.Position.X;
            }
        }
        """, "replaced\nreplaced\noriginal\n", 128);

    [Fact]
    public Task PrimitiveGcFieldsUseTheSameCallAbiAsLinearLocals() => GcTests.AssertExecution("""
        public class Holder { public byte Small; public long Wide; public float Number; }
        public static class Program {
            static void Set<T>(ref T location, T value) { location = value; }
            static ref T Forward<T>(ref T location) => ref location;
            public static int Main() {
                var holder = new Holder();
                Set(ref holder.Small, (byte)255); Set(ref holder.Wide, 4294967299L); Forward(ref holder.Number) = 3.5f;
                System.Console.WriteLine(holder.Small); System.Console.WriteLine(holder.Wide); System.Console.WriteLine(holder.Number);
                return 42;
            }
        }
        """, "255\n4294967299\n3.5\n", 42);

    [Fact]
    public Task NullManagedReferencesAreDistinctFromSlotsContainingNullAndThrowOnAccess() => GcTests.AssertExecution("""
        using System;
        using System.Runtime.CompilerServices;
        public struct Event { public string Text; public int Value; }
        public class Holder { public string Text; public Event Event; }
        public static class Program {
            static ref T Missing<T>() => ref Unsafe.NullRef<T>();
            static int Read(ref int value) => value;
            public static int Main() {
                var holder = new Holder(); string text = null; int number = 0; int caught = 0;
                Console.WriteLine(Unsafe.IsNullRef(ref text));
                Console.WriteLine(Unsafe.IsNullRef(ref holder.Event));
                Console.WriteLine(Unsafe.IsNullRef(ref number));
                Console.WriteLine(Unsafe.IsNullRef(ref Missing<Event>()));
                Console.WriteLine(Unsafe.IsNullRef(ref Missing<string>()));
                try { number = Read(ref Missing<int>()); } catch (NullReferenceException) { caught++; }
                try { Missing<string>() = "bad"; } catch (NullReferenceException) { caught++; }
                try { Console.WriteLine(Missing<string>()); } catch (NullReferenceException) { caught++; }
                try { Missing<Event>().Value = 9; } catch (NullReferenceException) { caught++; }
                try { Event value = Missing<Event>(); Console.WriteLine(value.Value); } catch (NullReferenceException) { caught++; }
                return caught;
            }
        }
        """, "False\nFalse\nFalse\nTrue\nTrue\n", 5);
}
