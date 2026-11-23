using Xui.Compiler.XuiNet.CodeFlow;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.CodeFlow;

public class StackTreeTests
{
    [Fact]
    public void PushInternsAndPopReturnsTheExistingPrefix()
    {
        var tree = new StackTree();
        var integer = tree.Empty.Push(StackType.Int32);
        var floating = integer.Push(StackType.Float);
        Assert.Same(integer, tree.Empty.Push(StackType.Int32));
        Assert.Same(floating, integer.Push(StackType.Float));
        Assert.Same(integer, floating.Pop());
        Assert.Same(tree.Empty, integer.Pop());
        Assert.Equal(3, tree.States.Count);
        Assert.Same(floating, Assert.Single(integer.Children));
    }

    [Fact]
    public void TheEntirePrefixDeterminesIdentity()
    {
        var tree = new StackTree();
        var first = tree.Empty.Push(StackType.Int32).Push(StackType.Float);
        var second = tree.Empty.Push(StackType.Int64).Push(StackType.Float);
        Assert.NotSame(first, second);
        Assert.Equal(first.Type, second.Type);
        Assert.NotSame(first, new StackTree().Empty.Push(StackType.Int32).Push(StackType.Float));
        Assert.Throws<InvalidProgramException>(() => tree.Empty.Pop());
    }
}
