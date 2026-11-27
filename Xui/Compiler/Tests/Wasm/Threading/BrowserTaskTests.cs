using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xui.Compiler.XuiNet.DotNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

public class BrowserTaskTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public Task IteratorsKeepIndependentCursorsAcrossSameAgentCallbacks(OptimizationLevel optimization) => Run("""
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        public static class Program {
            static Task<int> pending;
            static int check;
            static IEnumerable<int> Numbers() { yield return 10; yield return 20; yield return 12; }
            public static int Main() { pending=Compute(); return check; }
            static async Task<int> Compute() {
                int thread=Environment.CurrentManagedThreadId;
                var numbers=Numbers();
                using var first=numbers.GetEnumerator();
                first.MoveNext();
                int sum=await Task.Run(() => {
                    int result=0;
                    foreach(int number in numbers)result+=number;
                    if(Environment.CurrentManagedThreadId==thread && thread>0)check++;
                    return result;
                });
                if(first.Current==10 && first.MoveNext() && first.Current==20)check++;
                if(Environment.CurrentManagedThreadId==thread)check++;
                return sum;
            }
            public static int Done() => pending.IsCompleted ? 1 : 0;
            public static int Result() => pending.Result;
            public static int Check() => check;
        }
        """, optimization, 42, 3);

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public Task RepeatedAwaitsKeepStateAndRunFinally(OptimizationLevel optimization) => Run("""
        using System;
        using System.Threading.Tasks;
        public static class Program {
            static Task<int> pending;
            static int counter;
            public static int Main() { pending = Compute(); return counter; }
            static async Task<int> Compute() {
                int value = 2;
                try {
                    value += await Task.Run(() => { counter++; return 10; });
                    value += await Task.Run(() => { counter++; return 30; }).ConfigureAwait(false);
                    value += await Task.FromResult(0);
                    return value;
                } finally { counter += 10; }
            }
            public static int Done() => pending.IsCompleted ? 1 : 0;
            public static int Result() => pending.Result;
            public static int Check() => counter;
        }
        """, optimization, 42, 12);

    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public Task FaultsUnwrapForAwaitAndRemainAggregatedForResult(OptimizationLevel optimization) => Run("""
        using System;
        using System.Threading.Tasks;
        public static class Program {
            static Task<int> pending;
            static Exception error = new InvalidOperationException("broken");
            static int check;
            public static int Main() { pending = Compute(); return check; }
            static async Task<int> Compute() {
                Task<int> fault = Task.Run(new Func<int>(() => throw error));
                try { await fault; } catch (InvalidOperationException e) { if (ReferenceEquals(e, error)) check++; }
                if (fault.IsFaulted && !fault.IsCanceled && !fault.IsCompletedSuccessfully) check++;
                if (ReferenceEquals(fault.Exception.InnerException, error)) check++;
                try { _ = fault.Result; }
                catch (SystemException) { check = -100; }
                catch (AggregateException e) {
                    if (ReferenceEquals(e.InnerException, error) && e.Message == "One or more errors occurred. (broken)") check++;
                }
                return 42;
            }
            public static int Done() => pending.IsCompleted ? 1 : 0;
            public static int Result() => pending.Result;
            public static int Check() => check;
        }
        """, optimization, 42, 4);

    [Fact]
    public Task SharedTasksResumeEveryWaiterAndLazyCachesValuesAndFailures() => Run("""
        using System;
        using System.Threading.Tasks;
        public static class Program {
            static Task<int> pending;
            static int check;
            public static int Main() { pending = Compute(); return check; }
            static async Task<int> Wait(Task<int> task) { int value = await task; check++; return value; }
            static async Task Work() { await Task.Run(() => { check++; }); await Task.CompletedTask; }
            static async Task<int> Compute() {
                var shared = Task.Run(() => 21);
                var first = Wait(shared); var second = Wait(shared);
                int sum = await first + await second;
                await Work().ConfigureAwait(false);
                int calls = 0;
                var lazy = new Lazy<int>(() => { calls++; return 42; });
                if (!lazy.IsValueCreated && lazy.Value == 42 && lazy.Value == 42 && calls == 1 && lazy.IsValueCreated) check++;
                Exception error = new InvalidOperationException("cached");
                var failed = new Lazy<int>(() => { calls++; throw error; });
                for (int i = 0; i < 2; i++) {
                    try { _ = failed.Value; } catch (InvalidOperationException e) { if (ReferenceEquals(e,error)) check++; }
                }
                if (calls == 2 && !failed.IsValueCreated) check++;
                try { await Task.FromException(error); } catch (InvalidOperationException e) { if (ReferenceEquals(e,error)) check++; }
                try { await Task.FromException<int>(error); } catch (InvalidOperationException e) { if (ReferenceEquals(e,error)) check++; }
                return sum;
            }
            public static int Done() => pending.IsCompleted ? 1 : 0;
            public static int Result() => pending.Result;
            public static int Check() => check;
        }
        """, OptimizationLevel.Release, 42, 9);

    internal static async Task Run(string source, OptimizationLevel optimization, int result, int check)
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile(source, optimizationLevel: optimization));
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.Tasks.Task).Assembly.Location));
        using var framework = new Frontend.Framework("System.Collections.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies],
            BrowserTasks.Replacements(framework.Assemblies.Single(a => a.Identity.Name == "System.Private.CoreLib"), runtime));
        var exports = new[] { "Done", "Result", "Check" }.ToDictionary(name => name, name =>
            MethodIdentity.FromDefinition(app.ManifestModule, app.ManifestModule.Methods.Single(h => app.ManifestModule.GetMethod(h).Name == name)));
        byte[] wasm;
        try { wasm = new WasmEmitter().Emit(app, namedExports: exports); }
        catch (Exception error) { throw new InvalidOperationException($"{error.Message}\nMethod: {error.Data["Xui.Method"]}\n{error.Data["Xui.InstructionGraph"]}", error); }
        var start = new ProcessStartInfo("node") { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("--input-type=module"); start.ArgumentList.Add("--eval");
        start.ArgumentList.Add(JavaScript.Source + "\n" + $$"""
            import assert from 'node:assert/strict';
            const chunks = []; for await (const chunk of process.stdin) chunks.push(chunk);
            const module = await WebAssembly.compile(Buffer.concat(chunks), {builtins:['js-string']});
            const {exports:e} = await WebAssembly.instantiate(module, globalThis.xuiRuntimeImports);
            assert.equal(e.main(),0, 'Task.Run must be deferred');
            assert.equal(e.Done(),0, 'Await must suspend');
            const deadline = Date.now() + 5000;
            while (!e.Done() && Date.now() < deadline) await new Promise(resolve => setTimeout(resolve, 1));
            assert.equal(e.Done(),1, 'Task did not finish');
            assert.equal(e.Result(),{{result}});
            assert.equal(e.Check(),{{check}});
            """);
        using var node = Process.Start(start)!;
        var stdout = node.StandardOutput.ReadToEndAsync(); var stderr = node.StandardError.ReadToEndAsync();
        await node.StandardInput.BaseStream.WriteAsync(wasm); node.StandardInput.Close();
        await node.WaitForExitAsync();
        Assert.True(node.ExitCode == 0, await stderr); Assert.Equal("", await stdout);
    }
}
