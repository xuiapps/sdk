using Xui.Apps.TestApp.Examples;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>Exercises a production example in its real mobile page, independently of unrelated catalogue blockers.</summary>
public static class CataloguePage
{
    public static View Create<T>(MobilePerformanceMetrics metrics) where T : Example, new()
    {
        var navigation = new MobileNavigation(new View(), metrics, diagnosticsOverlay: null);
        ((Pages.IExampleNavigation)navigation).PushExamplePage<T>();
        return new TestAppDesignRoot(navigation);
    }

    public static int Sections(View root) => Find(root)?.SectionCount ?? 1;
    public static void Select(View root, int index) => Find(root)?.SelectSection(index);
    private static IExampleSectionNavigator? Find(View view)
    {
        if (view is IExampleSectionNavigator navigator) return navigator;
        for (int i = 0; i < view.Count; i++)
            if (Find(view[i]) is { } child) return child;
        return null;
    }
}
