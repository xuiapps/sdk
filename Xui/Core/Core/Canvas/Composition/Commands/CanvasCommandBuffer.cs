using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xui.Core.Canvas.Composition.Commands.Records;

namespace Xui.Core.Canvas.Composition.Commands;

/// <summary>Reusable pooled binary records and inline payloads, with a pooled managed resource table.</summary>
/// <remarks>Not thread-safe. Dispose the buffer to return its rented storage. Clear retains capacity
/// for the next recording. Resource objects are retained by reference and are never disposed here.</remarks>
public sealed partial class CanvasCommandBuffer : IDisposable
{
    private readonly ArrayPool<byte> pool;
    private byte[] commands = Array.Empty<byte>();
    private readonly ArrayPool<object?> resourcePool;
    private object?[] resources = Array.Empty<object?>();
    private int resourceCount;
    private int byteCount;
    private int replayDepth;
    private bool disposed;

    /// <summary>Creates a buffer backed by the shared byte and object array pools.</summary>
    public CanvasCommandBuffer() : this(ArrayPool<byte>.Shared) { }

    /// <summary>Creates a buffer backed by the supplied pool; storage is rented on first use.</summary>
    public CanvasCommandBuffer(ArrayPool<byte> pool) : this(pool, ArrayPool<object?>.Shared) { }

    /// <summary>Creates a buffer with supplied binary and resource pools, rented on first use.</summary>
    public CanvasCommandBuffer(ArrayPool<byte> pool, ArrayPool<object?> resourcePool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(resourcePool);
        this.pool = pool;
        this.resourcePool = resourcePool;
    }

    /// <summary>The number of complete commands in the stream.</summary>
    public int Count { get; private set; }

    /// <summary>Packed headers followed immediately by their length-prefixed payload elements.</summary>
    /// <remarks>The span is borrowed until the next mutation or disposal. Use MemoryMarshal.Read for
    /// unaligned headers. For text, dashes and gradients, Length specifies how many elements follow
    /// the header. Managed objects use ResourceRef IDs. These process-local bytes are not a portable
    /// serialization format.</remarks>
    public ReadOnlySpan<byte> Data
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return commands.AsSpan(0, byteCount);
        }
    }

    /// <summary>The number of retained object references in the shared resource list.</summary>
    public int ResourceCount => resourceCount;

    /// <summary>Resolves an ID from this buffer to its retained object.</summary>
    /// <remarks>The command determines the expected type, such as string or IImage.
    /// The list is not restricted to image resources; it can also retain future texture or
    /// other managed resource types. Null is allowed for a default font's family name.</remarks>
    public object? GetResource(ResourceRef reference)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if ((uint)reference.Id >= (uint)resourceCount) throw new ArgumentOutOfRangeException(nameof(reference));
        return resources[reference.Id];
    }

    private ResourceRef RetainResource<T>(T resource) where T : class?
    {
        EnsureResourceCapacity(checked(resourceCount + 1));
        var reference = new ResourceRef(resourceCount);
        resources[resourceCount++] = resource;
        return reference;
    }

    private void CheckMutation()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (replayDepth != 0) throw new InvalidOperationException("Cannot mutate a command buffer during its replay.");
    }

    /// <summary>Releases resource references while retaining rented storage for the next recording.</summary>
    public void Clear()
    {
        CheckMutation();
        Array.Clear(resources, 0, resourceCount);
        resourceCount = 0;
        Count = byteCount = 0;
    }

    /// <summary>Clears all object references and returns both rented arrays. Safe to call more than once.</summary>
    public void Dispose()
    {
        if (disposed) return;
        CheckMutation();
        disposed = true;
        resourceCount = 0;
        Count = byteCount = 0;
        var rented = commands;
        commands = Array.Empty<byte>();
        if (rented.Length != 0) pool.Return(rented, clearArray: true);
        var rentedResources = resources;
        resources = Array.Empty<object?>();
        ReturnResources(rentedResources);
    }

    internal void Add<T>(in T command, ReadOnlySpan<GradientStop> gradients=default,
        ReadOnlySpan<char> text=default, ReadOnlySpan<nfloat> numbers=default,
        Font font=default, object? resource=null) where T : unmanaged
    {
        CheckMutation();
        if (Unsafe.SizeOf<T>() != CommandLayout.Size(CommandLayout.Op(in command)))
            throw new ArgumentException("The record size does not match its canvas operation.", nameof(command));
        Append(in command, gradients, text, numbers, font, resource);
    }

    private void Write<T>(in T command, ReadOnlySpan<byte> payload = default) where T : unmanaged
    {
        var headerSize = Unsafe.SizeOf<T>();
        var end = checked(byteCount + headerSize + payload.Length);
        // A caller may pass a span borrowed from this buffer. Rebase it if growth returns
        // the old array to the pool before the append copies the payload.
        var borrowed = commands.AsSpan().Overlaps(payload, out var payloadOffset);
        EnsureCapacity(end);
        if (borrowed) payload = commands.AsSpan(payloadOffset, payload.Length);
        MemoryMarshal.Write(commands.AsSpan(byteCount), in command);
        payload.CopyTo(commands.AsSpan(byteCount + headerSize));
        byteCount = end;
        Count++;
    }

    private void EnsureCapacity(int length)
    {
        if (length <= commands.Length) return;
        var capacity = (int)Math.Min(int.MaxValue, Math.Max((long)length, Math.Max(256L, (long)commands.Length * 2)));
        var replacement = pool.Rent(capacity);
        commands.AsSpan(0, byteCount).CopyTo(replacement);
        var previous = commands;
        commands = replacement;
        if (previous.Length != 0) pool.Return(previous, clearArray: true);
    }

    private void EnsureResourceCapacity(int length)
    {
        if (length <= resources.Length) return;
        var capacity = (int)Math.Min(int.MaxValue, Math.Max((long)length, Math.Max(4L, (long)resources.Length * 2)));
        var replacement = resourcePool.Rent(capacity);
        // Pools may hand out uncleared storage from other borrowers. Never retain unused objects.
        Array.Clear(replacement);
        resources.AsSpan(0, resourceCount).CopyTo(replacement);
        var previous = resources;
        resources = replacement;
        ReturnResources(previous);
    }

    private void ReturnResources(object?[] rented)
    {
        if (rented.Length == 0) return;
        // Clear explicitly, even if the pool discards the array instead of caching it.
        Array.Clear(rented);
        resourcePool.Return(rented);
    }

    internal void Truncate(int count)
    {
        CheckMutation();
        if ((uint)count > (uint)Count) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == Count) return;
        var offset = 0;
        var resourceCount = 0;
        for (var i = 0; i < count; i++)
        {
            var record = Data[offset..];
            CountResources(record, ref resourceCount);
            offset += CommandLayout.Size(record);
        }
        byteCount = offset;
        Count = count;
        Array.Clear(resources, resourceCount, this.resourceCount - resourceCount);
        this.resourceCount = resourceCount;
    }

    /// <summary>Copies the binary stream and resource references into this buffer's owned storage.</summary>
    public void CopyFrom(CanvasCommandBuffer source)
    {
        CheckMutation();
        ArgumentNullException.ThrowIfNull(source);
        ObjectDisposedException.ThrowIf(source.disposed, source);
        if (ReferenceEquals(this, source)) return;
        EnsureCapacity(source.byteCount);
        EnsureResourceCapacity(source.resourceCount);
        source.Data.CopyTo(commands);
        byteCount = source.byteCount;
        Count = source.Count;
        if (source.resourceCount < resourceCount)
            Array.Clear(resources, source.resourceCount, resourceCount - source.resourceCount);
        source.resources.AsSpan(0, source.resourceCount).CopyTo(resources);
        resourceCount = source.resourceCount;
    }

    internal bool Same(CanvasCommandBuffer other)
    {
        if (Count != other.Count || resourceCount != other.resourceCount) return false;
        var a = Data; var b = other.Data;
        for (var i = 0; i < Count; i++)
        {
            var op = MemoryMarshal.Read<Command>(a).Op;
            if (op != MemoryMarshal.Read<Command>(b).Op) return false;
            var sizeA = CommandLayout.Size(a); var sizeB = CommandLayout.Size(b);
            if (sizeA != sizeB || !SameRecord(op, a, b) || !SamePayload(op, a[..sizeA], b[..sizeB])) return false;
            a = a[sizeA..]; b = b[sizeB..];
        }
        // Typed comparisons preserve NaN/signed-zero semantics without comparing struct padding
        // or boxing values. Transform operands are ignored because native placement owns them.
        for (var i = 0; i < resourceCount; i++)
        {
            var left = resources[i]; var right = other.resources[i];
            if (ReferenceEquals(left, right)) continue;
            // Strings compare by value; mutable resources compare by identity.
            if (left is not string text || right is not string otherText ||
                !string.Equals(text, otherText, StringComparison.Ordinal)) return false;
        }
        return true;
    }
}
