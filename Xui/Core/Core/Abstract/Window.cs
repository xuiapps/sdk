using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.Debug;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;

namespace Xui.Core.Abstract;

/// <summary>
/// Represents an abstract cross-platform application window in Xui.
/// Handles input, rendering, layout, and software keyboard integration.
/// </summary>
/// <remarks>
/// This class connects the abstract UI framework with the underlying platform window,
/// acting as a root container for layout and visual composition. Subclasses may override
/// specific input or rendering behaviors as needed.
/// </remarks>
public class Window : Abstract.IWindow, Abstract.IWindow.ISoftKeyboard, IServiceProvider, IDisposable
{
    /// <summary>The service provider for this window.</summary>
    public IServiceProvider Context { get; }

    /// <inheritdoc/>
    /// Resolves platform and middleware services through the actual-window chain, then
    /// falls back to this window's DI scope. The chain is strictly forward: actual
    /// windows must never consult their abstract event receiver for services.
    /// For <see cref="IOverlay"/>, falls back to the cross-platform in-window overlay
    /// when neither DI nor the platform provide one.
    public virtual object? GetService(Type serviceType)
    {
        var service = this.Actual.GetService(serviceType);

        if (service is not null)
            return service;

        // Provide in-window overlay fallback when no platform implementation is registered.
        if (serviceType == typeof(IOverlay))
        {
            var overlay = new PopupOverlay(this.RootView);
            this.DisposeQueue.Add(overlay);
            return overlay;
        }

        return null;
    }

    /// <summary>A list of disposables disposed when the window closes.</summary>
    public List<IDisposable> DisposeQueue { get; } = [];

    private bool disposed;
    private bool platformClosed;

    /// <summary>
    /// When <c>true</c> (the default), closing the window calls <see cref="Dispose()"/>.
    /// Set to <c>false</c> for windows that survive being closed and reopened.
    /// </summary>
    public bool DestroyOnClose { get; set; } = true;

    /// <summary>The platform runtime used by this window.</summary>
    public IRuntime Runtime { get; }

    /// <summary>
    /// Gets the underlying platform-specific window instance.
    /// </summary>
    public Actual.IWindow Actual { get; }

    /// <inheritdoc/>
    public virtual Rect DisplayArea { get; set; }

    /// <inheritdoc/>
    public virtual Rect SafeArea { get; set; }

    /// <inheritdoc/>
    public virtual nfloat ScreenCornerRadius { get; set; }

    /// <summary>
    /// Gets the text measure context for this window, used for hit-testing text
    /// positions during pointer events. Null on platforms that do not provide one.
    /// </summary>
    public virtual ITextMeasureContext? TextMeasureContext => this.Actual.TextMeasureContext;

    /// <summary>The root view that hosts the window's content hierarchy.</summary>
    public RootView RootView { get; }

    /// <summary>
    /// Stable name used to identify this window's render surface in instrumentation.
    /// </summary>
    public string InstrumentationName => this.GetType().Name;

    /// <summary>The instrumentation sink created for this window's render surface, if enabled.</summary>
    public IRenderSurfaceInstrumentsSink? RenderSurfaceInstruments { get; }

    /// <summary>Sets the root content view of this window.</summary>
    public View Content { init => this.RootView.Content = value; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Window"/> class.
    /// This creates the backing platform window.
    /// </summary>
    /// <param name="context">
    /// An optional scoped <see cref="IServiceProvider"/> for this window.
    /// If it implements <see cref="IDisposable"/>, it will be disposed when the window closes.
    /// </param>
    public Window(IServiceProvider context)
    {
        this.Context = context!;
        this.Runtime = (IRuntime)this.Context.GetService(typeof(IRuntime))!;
        this.Actual = this.CreateActualWindow();
        var instruments = this.Context.GetService(typeof(IInstruments)) as IInstruments;
        this.RenderSurfaceInstruments = instruments?.CreateSink().CreateRenderSurface(this.InstrumentationName);
        this.RootView = new RootView(this);
    }

    /// <summary>
    /// Gets or sets the window title (where supported by the platform).
    /// </summary>
    public string Title
    {
        get => this.Actual.Title;
        set => this.Actual.Title = value;
    }

    /// <summary>
    /// Requests that the soft keyboard be shown or hidden (on supported platforms).
    /// </summary>
    public bool RequireKeyboard
    {
        get => this.Actual.RequireKeyboard;
        set => this.Actual.RequireKeyboard = value;
    }

    /// <summary>
    /// Makes the window visible and adds it to the list of open windows.
    /// </summary>
    public void Show()
    {
        this.Actual.Show();
        HotReload.OnReload += this.Invalidate;
    }

    /// <inheritdoc/>
    public virtual void Render(ref RenderEventRef renderEventRef)
    {
        var rect = renderEventRef.Rect;
        // using var trace = Runtime.CurrentInstruments.Trace(Scope.Rendering, LevelOfDetail.Essential,
        //     $"Window.Render Rect({rect.X:F1}, {rect.Y:F1}, {rect.Width:F1}, {rect.Height:F1})");
        ((IContent)this.RootView).Update(ref renderEventRef);
    }

    /// <inheritdoc/>
    public virtual void WindowHitTest(ref WindowHitTestEventRef evRef)
    {
        // Subclasses may override to support custom window chrome or drag regions.
    }

    /// <inheritdoc/>
    public virtual bool Closing()
    {
        // Runtime.CurrentInstruments.Log(Scope.Application, LevelOfDetail.Essential,
        //     $"Window.Closing {this.GetType().Name}");
        return true;
    }

    /// <inheritdoc/>
    public virtual void Closed()
    {
        // Runtime.CurrentInstruments.Log(Scope.Application, LevelOfDetail.Essential,
        //     $"Window.Closed {this.GetType().Name}");
        HotReload.OnReload -= this.Invalidate;
        platformClosed = true;
        if (DestroyOnClose)
            Dispose();
    }

    /// <summary>
    /// Releases all managed resources owned by this window.
    /// If the platform window is still open, it is closed first.
    /// Items in <see cref="DisposeQueue"/> are disposed in order.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <param name="disposing">
    /// <c>true</c> when called from <see cref="Dispose()"/>; <c>false</c> from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        disposed = true;

        if (disposing)
        {
            this.RootView.FocusedView = null;
            if (!platformClosed)
                this.Actual.Close();

            foreach (var item in DisposeQueue) item.Dispose();
            DisposeQueue.Clear();
            this.RenderSurfaceInstruments?.Dispose();
        }
    }

    /// <summary>
    /// Creates the platform-specific window for this abstract window.
    /// </summary>
    /// <returns>The platform implementation of <see cref="Actual.IWindow"/>.</returns>
    protected virtual Actual.IWindow CreateActualWindow() => this.Runtime.CreateWindow(this, this.Context);

    /// <summary>
    /// Requests a visual invalidation/redraw of this window.
    /// </summary>
    public virtual void Invalidate()
    {
        // Runtime.CurrentInstruments.Log(Scope.ViewState, LevelOfDetail.Info,
        //     $"Window.Invalidate");
        this.Actual.Invalidate();
    }

    /// <inheritdoc/>
    public virtual void OnMouseDown(ref MouseDownEventRef e)
    {
        // Runtime.CurrentInstruments.Log(Scope.Input, LevelOfDetail.Normal,
        //     $"MouseDown ({e.Position.X:F1}, {e.Position.Y:F1}) Button={e.Button}");
        e.TextMeasure = this.TextMeasureContext;
        ((IContent)this.RootView).OnMouseDown(ref e);
    }

    /// <inheritdoc/>
    public virtual void OnMouseMove(ref MouseMoveEventRef e)
    {
        // Runtime.CurrentInstruments.Log(Scope.Input, LevelOfDetail.Diagnostic,
        //     $"MouseMove ({e.Position.X:F1}, {e.Position.Y:F1})");
        e.TextMeasure = this.TextMeasureContext;
        ((IContent)this.RootView).OnMouseMove(ref e);
    }

    /// <inheritdoc/>
    public virtual void OnMouseUp(ref MouseUpEventRef e)
    {
        // Runtime.CurrentInstruments.Log(Scope.Input, LevelOfDetail.Normal,
        //     $"MouseUp ({e.Position.X:F1}, {e.Position.Y:F1}) Button={e.Button}");
        e.TextMeasure = this.TextMeasureContext;
        ((IContent)this.RootView).OnMouseUp(ref e);
    }

    /// <inheritdoc/>
    public virtual void OnScrollWheel(ref ScrollWheelEventRef e)
    {
        ((IContent)this.RootView).OnScrollWheel(ref e);
    }

    /// <inheritdoc/>
    public virtual void OnTouch(ref TouchEventRef e)
    {
        e.TextMeasure = this.TextMeasureContext;
        ((IContent)this.RootView).OnTouch(ref e);
    }

    /// <inheritdoc/>
    public virtual void OnKeyDown(ref KeyEventRef e)
    {
        ((IContent)this.RootView).OnKeyDown(ref e);
    }

    /// <inheritdoc/>
    public virtual void OnChar(ref KeyEventRef e)
    {
        ((IContent)this.RootView).OnChar(ref e);
    }

    /// <inheritdoc/>
    public virtual void OnAnimationFrame(ref FrameEventRef e)
    {
        ((IContent)this.RootView).OnAnimationFrame(ref e);
    }

    /// <inheritdoc/>
    public virtual bool HasText => this.RootView.FocusedView is ITextInputClient { HasText: true };

    /// <inheritdoc/>
    public virtual bool IsSecureTextEntry => this.RootView.FocusedView is ITextInputClient { IsPassword: true };

    /// <inheritdoc/>
    public virtual void InsertText(ref InsertTextEventRef eventRef)
    {
        if (this.RootView.FocusedView is ITextEditingClient client)
        {
            if (eventRef.Text is "\n" or "\r" or "\r\n")
            {
                client.PerformInputAction(Xui.Core.UI.Input.TextInputAction.Default);
                return;
            }
            client.Editor.Insert(eventRef.Text);
            return;
        }
        foreach (var character in eventRef.Text)
        {
            var e = new KeyEventRef { Character = character };
            ((IContent)this.RootView).OnChar(ref e);
        }
    }

    /// <inheritdoc/>
    public virtual void DeleteBackwards(ref DeleteBackwardsEventRef eventRef)
    {
        var e = new KeyEventRef { Key = VirtualKey.Back };
        ((IContent)this.RootView).OnKeyDown(ref e);
    }
}
