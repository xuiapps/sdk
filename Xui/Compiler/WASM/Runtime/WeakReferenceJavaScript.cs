namespace Xui.Compiler.WASM.Runtime;

/// <summary>Optional WeakRef-backed imports for the explicit browser weak-reference profile.</summary>
public static class WeakReferenceJavaScript
{
    public static string Source { get; } = """
        function createWeakReferenceImports() {
            const handles = new WeakMap();
            return Object.freeze({
                set(owner, target) { handles.set(owner, target === null ? null : new WeakRef(target)); },
                get(owner) { return handles.get(owner)?.deref() ?? null; }
            });
        }
        """;
}
