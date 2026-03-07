namespace Xui.Core.UI.Layer;

/// <summary>
/// Zero-allocation click handler for <see cref="ButtonLayer{THost,TAction}"/>.
/// Implement as a private nested struct inside the owning view so <see cref="Execute"/>
/// receives the fully-typed host and can call any method on it without closures.
/// </summary>
public interface IButtonAction<in THost>
    where THost : ILayerHost
{
    void Execute(THost host);
}
