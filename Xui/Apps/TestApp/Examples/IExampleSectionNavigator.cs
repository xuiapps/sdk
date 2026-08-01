namespace Xui.Apps.TestApp.Examples;

/// <summary>Lets a mobile example expose a finite set of selectable sub-examples.</summary>
internal interface IExampleSectionNavigator
{
    bool ShowsMobileSectionControls => true;

    event Action? SectionChanged;

    int SectionCount { get; }

    int SelectedSectionIndex { get; }

    string GetSectionName(int index);

    void SelectSection(int index);
}
