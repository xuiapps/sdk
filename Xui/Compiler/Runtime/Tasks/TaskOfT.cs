using System.Runtime.CompilerServices;

namespace Xui.Compiler.Runtime.Tasks;

/// <summary>A task carrying a value with the ordinary managed value-copy semantics.</summary>
[AsyncMethodBuilder(typeof(AsyncTaskMethodBuilder<>))]
public class Task<T> : Task
{
    private T result = default!;
    public T Result { get { GetResult(aggregate: true); return result; } }
    internal void Complete(T value) { result = value; Complete(); }
    internal T AwaitResult() { GetResult(aggregate: false); return result; }
    public new TaskAwaiter<T> GetAwaiter() => new(this);
    public new ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext) => new(this);
}
