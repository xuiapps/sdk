using System.Runtime.CompilerServices;

namespace Xui.Compiler.Runtime.Tasks;

public readonly struct ConfiguredTaskAwaitable(Task task)
{
    public ConfiguredTaskAwaiter GetAwaiter() => new(task);
    public readonly struct ConfiguredTaskAwaiter(Task task) : ICriticalNotifyCompletion
    {
        public bool IsCompleted => task.IsCompleted;
        public void GetResult() => task.GetResult(aggregate: false);
        public void OnCompleted(Action continuation) => task.Continue(continuation);
        public void UnsafeOnCompleted(Action continuation) => task.Continue(continuation);
    }
}

public readonly struct ConfiguredTaskAwaitable<T>(Task<T> task)
{
    public ConfiguredTaskAwaiter GetAwaiter() => new(task);
    public readonly struct ConfiguredTaskAwaiter(Task<T> task) : ICriticalNotifyCompletion
    {
        public bool IsCompleted => task.IsCompleted;
        public T GetResult() => task.AwaitResult();
        public void OnCompleted(Action continuation) => task.Continue(continuation);
        public void UnsafeOnCompleted(Action continuation) => task.Continue(continuation);
    }
}
