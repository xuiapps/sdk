namespace Xui.Compiler.Runtime;

/// <summary>A weak reference to a managed WASM object on the current browser agent.</summary>
public class WeakReference
{
    public WeakReference(object? target) : this(target, false) { }
    public WeakReference(object? target, bool trackResurrection)
    {
        if (trackResurrection) throw new NotSupportedException("Resurrection tracking requires managed finalizers.");
        Target = target;
    }
    public virtual object? Target { get => NativeWeakReferences.Get(this); set => WeakTargets.Set(this, value); }
    public virtual bool IsAlive => NativeWeakReferences.Get(this) is not null;
    public virtual bool TrackResurrection => false;
}

/// <summary>A typed weak reference; retrieval keeps the returned target alive through normal managed references.</summary>
public sealed class WeakReference<T> where T : class?
{
    public WeakReference(T target) : this(target, false) { }
    public WeakReference(T target, bool trackResurrection)
    {
        if (trackResurrection) throw new NotSupportedException("Resurrection tracking requires managed finalizers.");
        SetTarget(target);
    }
    public bool TryGetTarget(out T target) { target = (T)NativeWeakReferences.Get(this)!; return target is not null; }
    public void SetTarget(T target) => WeakTargets.Set(this, target);
}

internal static class WeakTargets
{
    public static void Set(object owner, object? target)
    {
        if (target is string) throw new NotSupportedException("Weak references to native JavaScript strings require managed string identity.");
        NativeWeakReferences.Set(owner, target);
    }
}

internal static class NativeWeakReferences
{
    [RuntimeImport("xui-weak", "set")]
    public static void Set(object owner, object? target) => throw new NotSupportedException();
    [RuntimeImport("xui-weak", "get")]
    public static object? Get(object owner) => throw new NotSupportedException();
}
