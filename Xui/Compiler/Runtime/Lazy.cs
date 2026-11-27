namespace Xui.Compiler.Runtime;

/// <summary>A single-agent factory with cached values/failures and explicit recursive-entry rejection.</summary>
public sealed class Lazy<T>
{
    private Func<T>? factory;
    private T value = default!;
    private Exception? error;
    private bool creating;
    public bool IsValueCreated { get; private set; }

    public Lazy(Func<T> valueFactory) { ArgumentNullException.ThrowIfNull(valueFactory); factory = valueFactory; }
    public T Value
    {
        get
        {
            if (IsValueCreated) return value;
            if (error is not null) throw error;
            if (creating) throw new InvalidOperationException("ValueFactory attempted to access the Value property of this instance.");
            creating = true;
            try { value = factory!(); IsValueCreated = true; factory = null; }
            catch (Exception exception) { error = exception; factory = null; throw; }
            finally { creating = false; }
            return value;
        }
    }
}
