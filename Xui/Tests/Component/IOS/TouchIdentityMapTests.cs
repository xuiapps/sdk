using Xui.Runtime.IOS.Actual;
using Xunit;

namespace Xui.Tests.Component.IOS;

public class TouchIdentityMapTests
{
    [Fact]
    public void NativeHandlesReceiveDistinctStableIdsUntilReleased()
    {
        var contacts = new TouchIdentityMap();
        Assert.True(contacts.TryBegin(0x1000, out var first));
        Assert.True(contacts.TryBegin(0x2000, out var second));
        Assert.NotEqual(first, second);
        Assert.True(contacts.TryGet(0x1000, out var moved));
        Assert.Equal(first, moved);
        Assert.False(contacts.TryBegin(0x1000, out var duplicate));
        Assert.Equal(first, duplicate);
        // The caller releases ended contacts after dispatching the entire batch.
        Assert.True(contacts.TryBegin(0x3000, out var third));
        Assert.NotEqual(first, third); Assert.NotEqual(second, third);
        contacts.End(0x1000);
        Assert.True(contacts.TryBegin(0x4000, out var reused));
        Assert.Equal(first, reused);
        Assert.True(contacts.TryGet(0x2000, out var stillSecond));
        Assert.Equal(second, stillSecond);
        contacts.End(0x2000);
        Assert.False(contacts.TryGet(0x2000, out _));
    }
}
