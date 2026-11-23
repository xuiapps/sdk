using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.UI;
using Xui.GPU.Backends;
using Xui.GPU.Hardware;

namespace Xui.Apps.TestApp.Pages.FloorplanGpu;

/// <summary>Keeps the placeholder until upload, pipeline creation and GPU warmup have completed.</summary>
internal sealed class FloorplanGpuLoadingView(Task<FloorplanGpuModel> model)
    : FloorplanLoadingView<FloorplanGpuModel>(model, static data => new FloorplanGpuView(data), deferHydration: true)
{
    private Task<FloorplanGpuRenderer>? preparation;

    protected override Task PrepareAsync(FloorplanGpuModel data)
    {
        // Resolve services on the UI thread. The worker only touches immutable model data
        // and privately owned Metal resources, never the view tree or a drawing context.
        if (GetService(typeof(IGpuDevice)) is not IGpuResourceDevice device ||
            GetService(typeof(IShaderBackend)) is not IShaderBackend backend)
            return Task.CompletedTask;
        return preparation = Task.Run(() =>
        {
            var renderer = new FloorplanGpuRenderer(data);
            try { renderer.PrepareAndWarm(device, backend); return renderer; }
            catch { renderer.Dispose(); throw; }
        });
    }

    protected override View CreateView(FloorplanGpuModel data)
    {
        var renderer = preparation?.GetAwaiter().GetResult();
        preparation = null; // Transfer ownership to the hydrated view.
        return new FloorplanGpuView(data, renderer);
    }

    protected override void OnDeactivate()
    {
        if (preparation is { } abandoned)
        {
            preparation = null;
            _ = DisposeWhenReady(abandoned);
            ResetPreparation();
        }
        base.OnDeactivate();
    }

    private static async Task DisposeWhenReady(Task<FloorplanGpuRenderer> task)
    {
        try { (await task.ConfigureAwait(false)).Dispose(); }
        catch { /* The worker already disposed resources when preparation failed. */ }
    }
}
