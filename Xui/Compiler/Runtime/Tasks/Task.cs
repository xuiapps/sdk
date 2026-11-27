using System.Runtime.CompilerServices;

namespace Xui.Compiler.Runtime.Tasks;

/// <summary>A completion object owned by one browser agent; pending tasks never block that agent.</summary>
[AsyncMethodBuilder(typeof(AsyncTaskMethodBuilder))]
public class Task : IDisposable
{
    private int state;
    private Exception? error;
    private List<Action>? continuations;
    public bool IsCompleted => state != 0;
    public bool IsCompletedSuccessfully => state == 1;
    public bool IsFaulted => state == 2;
    public bool IsCanceled => false;
    public AggregateException? Exception => error is null ? null : new AggregateException("One or more errors occurred.", error);
    public static Task CompletedTask { get; } = Success();

    private static Task Success() { var task = new Task(); task.Complete(); return task; }

    public static Task Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var task = new Task();
        NativeScheduler.Post(() => {
            try { action(); task.Complete(); }
            catch (Exception error) { task.Fail(error); }
        });
        return task;
    }

    public static Task<T> Run<T>(Func<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var task = new Task<T>();
        NativeScheduler.Post(() => {
            try { task.Complete(action()); }
            catch (Exception error) { task.Fail(error); }
        });
        return task;
    }

    public static Task<T> FromResult<T>(T value) { var task = new Task<T>(); task.Complete(value); return task; }
    public static Task FromException(Exception exception) { var task = new Task(); task.Fail(exception); return task; }
    public static Task<T> FromException<T>(Exception exception) { var task = new Task<T>(); task.Fail(exception); return task; }
    public TaskAwaiter GetAwaiter() => new(this);
    public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => new(this);

    internal void Continue(Action continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);
        if (IsCompleted) NativeScheduler.Post(continuation);
        else (continuations ??= new()).Add(continuation);
    }

    internal void Complete() => Finish(1, null);
    internal void Fail(Exception exception) { ArgumentNullException.ThrowIfNull(exception); Finish(2, exception); }

    private void Finish(int completedState, Exception? exception)
    {
        if (IsCompleted) throw new InvalidOperationException("The task has already completed.");
        error = exception; state = completedState;
        var pending = continuations; continuations = null;
        if (pending is not null)
            for (int i = 0; i < pending.Count; i++) NativeScheduler.Post(pending[i]);
    }

    internal void GetResult(bool aggregate)
    {
        if (!IsCompleted) throw new NotSupportedException("A pending task cannot block the browser agent.");
        if (error is not null)
        {
            if (aggregate) throw new AggregateException("One or more errors occurred.", error);
            throw error;
        }
    }

    public void Dispose()
    {
        if (!IsCompleted) throw new InvalidOperationException("Only a completed task can be disposed.");
    }
}
