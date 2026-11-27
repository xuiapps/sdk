using System.Runtime.CompilerServices;

namespace Xui.Compiler.Runtime.Tasks;

public struct AsyncTaskMethodBuilder
{
    private Task? task;
    public static AsyncTaskMethodBuilder Create() => default;
    public Task Task => task ??= new();
    public void SetResult() => Task.Complete();
    public void SetException(Exception exception) => Task.Fail(exception);
    public void SetStateMachine(IAsyncStateMachine stateMachine) => ArgumentNullException.ThrowIfNull(stateMachine);
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        _ = Task;
        Action continuation = new StateMachineContinuation<TStateMachine>(stateMachine).MoveNext;
        awaiter.OnCompleted(continuation);
    }
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        _ = Task;
        Action continuation = new StateMachineContinuation<TStateMachine>(stateMachine).MoveNext;
        awaiter.UnsafeOnCompleted(continuation);
    }
}

public struct AsyncTaskMethodBuilder<T>
{
    private Task<T>? task;
    public static AsyncTaskMethodBuilder<T> Create() => default;
    public Task<T> Task => task ??= new();
    public void SetResult(T result) => Task.Complete(result);
    public void SetException(Exception exception) => Task.Fail(exception);
    public void SetStateMachine(IAsyncStateMachine stateMachine) => ArgumentNullException.ThrowIfNull(stateMachine);
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        _ = Task;
        Action continuation = new StateMachineContinuation<TStateMachine>(stateMachine).MoveNext;
        awaiter.OnCompleted(continuation);
    }
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        _ = Task;
        Action continuation = new StateMachineContinuation<TStateMachine>(stateMachine).MoveNext;
        awaiter.UnsafeOnCompleted(continuation);
    }
}

/// <summary>Owns the state-machine value across a suspension; its builder retains the shared completion object.</summary>
internal sealed class StateMachineContinuation<TStateMachine>(TStateMachine state) where TStateMachine : IAsyncStateMachine
{
    public void MoveNext() => state.MoveNext();
}
