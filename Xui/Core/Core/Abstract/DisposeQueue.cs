namespace Xui.Core.Abstract;

/// <summary>
/// A lightweight accumulator of <see cref="IDisposable"/> instances that are all disposed together.
/// Use <c>+=</c> to register and <c>-=</c> to unregister disposables.
/// </summary>
public struct DisposeQueue
{
    private List<IDisposable>? items;

    public static DisposeQueue operator +(DisposeQueue queue, IDisposable item)
    {
        (queue.items ??= []).Add(item);
        return queue;
    }

    public static DisposeQueue operator -(DisposeQueue queue, IDisposable item)
    {
        queue.items?.Remove(item);
        return queue;
    }

    internal void DisposeAll()
    {
        if (items is null)
            return;
        foreach (var item in items)
            item.Dispose();
        items.Clear();
    }
}
