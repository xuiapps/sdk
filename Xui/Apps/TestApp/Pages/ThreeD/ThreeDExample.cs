using NFloat = System.Runtime.InteropServices.NFloat;
using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.ThreeD.Tests;
using Xui.Apps.TestApp.Pages.ThreeD.Tests.GpuHardwareCubeTest;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;

namespace Xui.Apps.TestApp.Pages.ThreeD;

/// <summary>A hardware 3D example with an opt-in software comparison.</summary>
public class ThreeDExample : Example, IMobileExamplePresentation, INavigationAware
{
    private readonly ThreeDTestPanel panel;

    public ThreeDExample()
    {
        Title = "3D Rendering";
        Content = panel = new ThreeDTestPanel();
    }

    public void UseMobilePresentation() { }

    public void OnNavigationStateChanged(in NavigationStateChange change) =>
        panel.IsLive = change.Phase == NavigationPhase.Appeared;

    public sealed class ThreeDTestPanel : ViewCollection
    {
        private CubeSurface demo = new GpuHardwareCubeTestView();
        private readonly Button rendererButton;
        private ExampleSettings? settings;
        private bool software;
        private bool isLive = true;

        public CubeSurface Demo => demo;

        public bool IsLive
        {
            get => isLive;
            set { isLive = value; demo.IsLive = value; }
        }

        public ThreeDTestPanel()
        {
            demo.Id = "ThreeDSurface";
            Add(demo);
            rendererButton = new Button
            {
                Id = "ThreeDRendererSwitch",
                Text = "Use software renderer",
                Variant = ButtonVariant.Outline,
                Clicked = SwitchRenderer,
            };
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            settings = GetService(typeof(ExampleSettings)) as ExampleSettings;
            if (settings is not null) settings.Changed += ApplySettings;
            ApplySettings();
        }

        protected override void OnDeactivate()
        {
            if (settings is not null) settings.Changed -= ApplySettings;
            settings = null;
            base.OnDeactivate();
        }

        private void ApplySettings()
        {
            bool show = settings?.ShowDeveloperFeatures == true;
            if (!show && software) SetRenderer(false);
            if (show && rendererButton.Parent is null) Add(rendererButton);
            else if (!show && rendererButton.Parent is not null) Remove(rendererButton);
            Invalidate();
        }

        private void SwitchRenderer()
        {
            if (settings?.ShowDeveloperFeatures == true && IsLive)
                SetRenderer(!software);
        }

        private void SetRenderer(bool useSoftware)
        {
            Remove(demo);
            software = useSoftware;
            demo = software ? new RotatingCubeTest() : new GpuHardwareCubeTestView();
            demo.Id = "ThreeDSurface";
            demo.IsLive = isLive;
            Add(demo);
            rendererButton.Text = software ? "Use hardware renderer" : "Use software renderer";
            Invalidate();
        }

        protected override void RenderCore(IContext context)
        {
            for (var i = 0; i < Count; i++)
            {
                var child = this[i];
                using var group = context.Group(new GroupOptions
                { Key = child, Bounds = child.Frame, ViewBacking = true });
                RenderChild(context, child);
            }
        }

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            demo.Measure((300, 300), context);
            if (rendererButton.Parent is not null) rendererButton.Measure(available, context);
            return available;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            var buttonSize = rendererButton.Parent is not null ? rendererButton.Measure(rect.Size, context) : Size.Empty;
            var totalHeight = 300 + (buttonSize.Height > 0 ? buttonSize.Height + 24 : 0);
            var top = rect.Y + NFloat.Max(0, (rect.Height - totalHeight) / 2);
            demo.Arrange((rect.X + (rect.Width - 300) / 2, top, 300, 300), context);
            if (rendererButton.Parent is not null)
                rendererButton.Arrange((rect.X + (rect.Width - buttonSize.Width) / 2, top + 324, buttonSize.Width, buttonSize.Height), context);
        }
    }
}
