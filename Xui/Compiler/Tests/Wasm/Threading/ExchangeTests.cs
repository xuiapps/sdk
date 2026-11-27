using System.Reflection.Metadata;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

[Collection(nameof(ManagedConsoleCollection))]
public class ExchangeTests
{
    [Fact]
    public void RuntimeExchangeObjectIsRecognizedWithoutReadingItsNativeBody()
    {
        using var framework=new Frontend.Framework();
        var module=framework.Assemblies[0].ManifestModule;
        var owner=module.Types.Single(h=>module.GetType(h).Namespace=="System.Threading" && module.GetType(h).Name=="Interlocked");
        var handle=module.GetType(owner).Methods.Single(h=>module.GetMethod(h).Name=="ExchangeObject");
        Assert.Null(module.GetMethod(handle).Body);
        var signature=Xui.Compiler.XuiNet.DotNet.Intrinsics.ReferenceInterlocked.Read(module,handle,null);
        Assert.NotNull(signature);
        Assert.Equal(SignatureTypeCode.Object,signature.Value.Result.Code);
        Assert.Equal(2,signature.Value.Parameters.Length);
    }

    [Fact]
    public Task ReferencesAndStringsReturnTheirOldValueAndReplaceTheSlot() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Threading;
        using System.Runtime.CompilerServices;
        class Item { }
        class Holder { public Item Value; public string Text; }
        struct Entry { public Item Value; public string Text; }
        public static class Program {
            static Item saved;
            static T Replace<T>(ref T value,T next) where T:class=>Interlocked.Exchange(ref value,next);
            public static int Main() {
                var first=new Item(); var second=new Item(); var local=first;
                Console.WriteLine(Replace(ref local,second)==first); Console.WriteLine(local==second);
                var holder=new Holder {Value=first,Text="old"};
                Console.WriteLine(Replace(ref holder.Value,second)==first); Console.WriteLine(holder.Value==second);
                saved=first; Console.WriteLine(Replace(ref saved,null)==first); Console.WriteLine(saved==null);
                var entries=new Entry[1]; entries[0].Value=first; entries[0].Text="array";
                Console.WriteLine(Replace(ref entries[0].Value,second)==first); Console.WriteLine(entries[0].Value==second);
                Console.WriteLine(Replace(ref entries[0].Text,"replaced")); Console.WriteLine(entries[0].Text);
                Console.WriteLine(Replace(ref holder.Text,"new")); Console.WriteLine(holder.Text);
                string text="local"; Console.WriteLine(Replace(ref text,null)); Console.WriteLine(text==null);
                byte[] bytes=new byte[3]; Console.WriteLine(Replace(ref bytes,null).Length); Console.WriteLine(bytes==null);
                try { Replace(ref Unsafe.NullRef<Item>(),second); } catch(NullReferenceException) { Console.WriteLine("null ref"); }
                try { Replace(ref Unsafe.NullRef<string>(),"x"); } catch(NullReferenceException) { Console.WriteLine("null string ref"); }
                return 42;
            }
        }
        """,string.Concat(Enumerable.Repeat("True\n",8))+"array\nreplaced\nold\nnew\nlocal\nTrue\n3\nTrue\nnull ref\nnull string ref\n",42);
}
