using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Equality;

[Collection(nameof(ManagedConsoleCollection))]
public class StringDictionaryTests
{
    [Fact]
    public Task DefaultStringKeysResizeAndRetainStructValues() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Collections.Generic;
        struct Value { public int X; public string Label; }
        public static class Program {
            public static int Main() {
                var map=new Dictionary<string,Value>();
                for(int i=0;i<150;i++) map.Add(i.ToString(),new Value {X=i,Label="value"});
                for(int i=0;i<150;i++) if(map[i.ToString()].X!=i || map[i.ToString()].Label!="value") return 1;
                map["😀"]=new Value {X=42,Label="unicode"};
                map.Remove("0");
                if(map.ContainsKey("0")) return 2;
                bool rejected=false; try { map.Add(null,default); } catch(ArgumentNullException) { rejected=true; }
                if(!rejected) return 3;
                return map["😀"].X;
            }
        }
        """, "", 42, System.Reflection.Assembly.Load("System.Collections"));

    [Fact]
    public Task CustomStringComparerSurvivesCollisionsAndResizing() => InlineArrayTests.AssertLinked("""
        using System.Collections.Generic;
        class Comparer : IEqualityComparer<string> {
            public bool Equals(string a,string b) => a==b;
            public int GetHashCode(string value) => 7;
        }
        public static class Program {
            public static int Main() {
                var comparer=new Comparer(); var map=new Dictionary<string,int>(comparer);
                for(int i=0;i<150;i++) map.Add(i.ToString(),i);
                for(int i=0;i<150;i++) if(map[i.ToString()]!=i) return 1;
                return map.Comparer==comparer ? 42 : 2;
            }
        }
        """, "", 42, System.Reflection.Assembly.Load("System.Collections"));
}
