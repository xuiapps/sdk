namespace Xui.Apps.TestApp.Pages;

/// <summary>Separates an ASCII lower-case letter followed by an upper-case letter in example identifiers.</summary>
internal static class ExampleDisplayName
{
    public static string Format(string name)
    {
        string result = "";
        int start = 0;
        for (int i = 1; i < name.Length; i++)
        {
            if (name[i - 1] is >= 'a' and <= 'z' && name[i] is >= 'A' and <= 'Z')
            {
                result = string.Concat(result, name.Substring(start, i - start), " ");
                start = i;
            }
        }
        return start == 0 ? name : string.Concat(result, name.Substring(start));
    }
}
