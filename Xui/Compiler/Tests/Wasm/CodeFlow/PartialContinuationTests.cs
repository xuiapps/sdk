using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

[Collection(nameof(ManagedConsoleCollection))]
public class PartialContinuationTests
{
    [Fact]
    public Task BreakCanPassThroughANestedLoopBeforeTheCommonExit() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static int Wrap(string text,int width) {
                int lines=0;
                for(int start=0;start<text.Length;) {
                    int end=start,accepted=start;
                    while(end<text.Length) {
                        int space=text.IndexOf(' ',end),next=space<0?text.Length:space;
                        if(next-start>width) {
                            if(accepted==start) {
                                accepted=start+1;
                                while(accepted<next && accepted+1-start<=width) accepted++;
                            }
                            break;
                        }
                        accepted=next;end=next+1;
                    }
                    lines++;start=accepted;
                    while(start<text.Length && text[start]==' ') start++;
                }
                return lines;
            }
            public static int Main() {
                Console.WriteLine(Wrap("aaaa bbbbbbbb cc",4));
                Console.WriteLine(Wrap("a b c",8));
                return 42;
            }
        }
        """, "4\n1\n",42);

    [Fact]
    public Task OnlyContinuingArmsContributeTheirStackAtTheJoin() => InlineArrayTests.AssertLinked("""
        using System;
        public class Item { public int Value; public Item(int value) { Value=value; } }
        public static class Program {
            static Item Choose(Item current,int direction,bool wrap,Item first,Item last,Item prev,Item next) {
                if(current is null) return direction>0 ? first : last;
                return direction>0 ? next ?? (wrap ? first : null) : prev ?? (wrap ? last : null);
            }
            public static int Main() {
                var first=new Item(20); var last=new Item(22);
                if(Choose(first,1,false,first,last,null,null)!=null) return 1;
                if(Choose(last,-1,false,first,last,null,null)!=null) return 2;
                if(Choose(first,1,true,first,last,null,last)!=last) return 3;
                return Choose(null,1,true,first,last,null,null).Value+Choose(first,-1,true,first,last,null,null).Value;
            }
        }
        """, "",42);

    [Fact]
    public Task SharedShortCircuitTailsWithinFinallyExecuteOnce() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static int effects;
            static int Run(bool a,bool b,bool c) {
                int result=0;
                try { if(a || b || c) { effects++; result=20; } else result=22; }
                finally { effects+=10; }
                return result;
            }
            public static int Main() {
                for(int i=0;i<8;i++) {
                    effects=0; int result=Run((i&1)!=0,(i&2)!=0,(i&4)!=0);
                    if(result!=(i==0?22:20) || effects!=(i==0?10:11)) return -1;
                }
                return 42;
            }
        }
        """, "",42);
}
