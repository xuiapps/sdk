using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class CollectionInterfaceTests
{
    [Fact]
    public Task CollectionCopyPreservesStructValuesAndExistingElementReferences() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        struct Item { public int Number; public string Text; }
        public static class Program {
            static void Copy(ICollection<Item> source,Item[] target,int index)=>source.CopyTo(target,index);
            public static int Main() {
                var source=new Item[] {new Item {Number=42,Text="original"}};
                var target=new Item[2]; ref Item alias=ref target[1];
                Copy(source,target,1); source[0].Number=99; source[0].Text="changed";
                Console.WriteLine(alias.Text); Console.WriteLine(alias.Number);
                var list=new List<Item>(); list.Add(new Item {Number=42,Text="class"});
                Copy(list,target,0); Console.WriteLine(target[0].Text);
                ICollection<Item> collection=source;
                Console.WriteLine(collection.Count); Console.WriteLine(collection.IsReadOnly);
                try { collection.CopyTo(null,0); } catch(ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { collection.CopyTo(target,-1); } catch(ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                try { collection.CopyTo(target,2); } catch(ArgumentException) { Console.WriteLine("space"); }
                return alias.Number;
            }
        }
        ""","original\n42\nclass\n1\nTrue\ndestinationArray\ndestinationIndex\nspace\n",42,
        System.Reflection.Assembly.Load("System.Collections"));

    [Fact]
    public Task ArrayEnumerationUsesCoreLibStateAndCopiesStructItems() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections;
        using System.Collections.Generic;
        public static class Program {
            static int Sum(IEnumerable<(int Number,string Text)> source) {
                int sum=0;
                foreach(var item in source) { sum+=item.Number; Console.WriteLine(item.Text); }
                return sum;
            }
            public static int Main() {
                var values=new (int,string)[] {(20,"a"),(22,"b")};
                IEnumerable<(int Number,string Text)> source=values;
                var iterator=source.GetEnumerator();
                try { var unused=iterator.Current; } catch(InvalidOperationException) { Console.WriteLine("before"); }
                iterator.MoveNext(); var copy=iterator.Current; copy.Number=99;
                Console.WriteLine(iterator.Current.Number);
                while(iterator.MoveNext()) {};
                try { var unused=iterator.Current; } catch(InvalidOperationException) { Console.WriteLine("after"); }
                ((IEnumerator)iterator).Reset(); Console.WriteLine(iterator.MoveNext());
                iterator.Dispose();
                return Sum(source)+Sum(Array.Empty<(int,string)>());
            }
        }
        ""","before\n20\nafter\nTrue\na\nb\n",42);

    [Fact]
    public Task PrimitiveAndStringCollectionCopiesPreserveValues() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        public static class Program {
            public static int Main() {
                var values=new int[] {20,22}; var target=new int[3];
                ICollection<int> collection=values;
                try { collection.Add(1); } catch(NotSupportedException) { Console.WriteLine("fixed add"); }
                try { collection.Remove(20); } catch(NotSupportedException) { Console.WriteLine("fixed remove"); }
                try { collection.Clear(); } catch(NotSupportedException) { Console.WriteLine("fixed clear"); }
                ((ICollection<int>)values).CopyTo(target,1);
                ((ICollection<int>)values).CopyTo(values,0);
                ((ICollection<int>)Array.Empty<int>()).CopyTo(target,3);
                var strings=new string[] {"text",null}; var text=new string[2];
                ((ICollection<string>)strings).CopyTo(text,0);
                Console.WriteLine(text[0]); Console.WriteLine(text[1]==null);
                return target[1]+target[2];
            }
        }
        ""","fixed add\nfixed remove\nfixed clear\ntext\nTrue\n",42);
}
