namespace Xui.Apps.TestApp.Pages.Floorplan;

/// <summary>Reads the SVG transform names and argument slices used by the floorplan loader.</summary>
/// <remarks>Preserves the former ASCII-name/parenthesized-argument matcher without a regex engine.</remarks>
internal ref struct SvgTransformReader(string text)
{
    private int offset;
    private int nameStart, nameLength, argumentStart, argumentLength;
    public string Name => text.Substring(nameStart, nameLength);
    public ReadOnlySpan<char> Arguments => text.AsSpan(argumentStart, argumentLength);

    public bool MoveNext()
    {
        while (offset < text.Length)
        {
            if (!Letter(text[offset])) { offset++; continue; }
            int start = offset;
            while (offset < text.Length && Letter(text[offset])) offset++;
            if (offset == text.Length || text[offset] != '(') continue;
            int arguments = ++offset;
            int end = text.IndexOf(')', arguments);
            if (end < 0) break;
            if (end == arguments) continue;
            nameStart = start;
            nameLength = arguments - start - 1;
            argumentStart = arguments;
            argumentLength = end - arguments;
            offset = end + 1;
            return true;
        }
        nameLength = argumentLength = 0;
        return false;
    }

    private static bool Letter(char value) => value is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
}
