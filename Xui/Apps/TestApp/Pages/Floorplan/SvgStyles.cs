using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Xui.Apps.TestApp.Pages.Floorplan;

// Intentionally scoped to the embedded study: presentation attributes, class/id/attribute
// selectors, descendants/children and inherited custom properties. Not a general web CSS engine.
internal sealed class SvgStyles
{
    private sealed record Rule(string[] Parts, KeyValuePair<string, string>[] Values, int Specificity, int Order);
    private readonly Rule[] rules;
    public SvgStyles(XElement root)
    {
        var list = new List<Rule>();
        foreach (var style in root.Elements().Where(e => e.Name.LocalName == "style"))
        {
            var css = Regex.Replace(style.Value, @"/\*.*?\*/", "", RegexOptions.Singleline);
            foreach (Match m in Regex.Matches(css, @"([^{}]+)\{([^{}]*)\}"))
            {
                var values = Declarations(m.Groups[2].Value);
                foreach (var selector in m.Groups[1].Value.Split(','))
                {
                    if (selector.Contains('@'))
                        continue;
                    var parts = Regex.Matches(selector.Trim(), @"(?:\[[^\]]+\]|[^\s>])+|>").Select(m => m.Value).ToArray();
                    int specificity = Regex.Matches(selector, @"#[\w-]+").Count * 100
                        + Regex.Matches(selector, @"\.[\w-]+|\[[^\]]+\]").Count * 10;
                    list.Add(new(parts, values, specificity, list.Count));
                }
            }
        }
        rules = list.OrderBy(r => r.Specificity).ThenBy(r => r.Order).ToArray();
    }
    public static KeyValuePair<string, string>[] Declarations(string value) => value.Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Split(':', 2)).Where(p => p.Length == 2)
        .Select(p => new KeyValuePair<string, string>(p[0].Trim(), p[1].Trim())).ToArray();
    public Dictionary<string, string> Resolve(List<XElement> ancestry, Dictionary<string, string> inherited)
    {
        var style = new Dictionary<string, string>(inherited) { ["display"] = "inline", ["opacity"] = "1" };
        var node = ancestry[^1];
        foreach (var a in node.Attributes())
            style[a.Name.LocalName] = a.Value;
        foreach (var rule in rules)
            if (Match(rule.Parts, rule.Parts.Length - 1, ancestry, ancestry.Count - 1))
                foreach (var p in rule.Values)
                    style[p.Key] = p.Value;
        foreach (var p in Declarations(node.Attribute("style")?.Value ?? ""))
            if (p.Value != "inherit")
                style[p.Key] = p.Value;
        return style;
    }
    public static string Get(Dictionary<string, string> style, string name, string fallback = "")
    {
        var value = style.GetValueOrDefault(name, fallback);
        for (int i = 0; i < 8 && value.StartsWith("var(", StringComparison.Ordinal); i++)
        {
            var parts = value[4..^1].Split(',', 2);
            value = style.GetValueOrDefault(parts[0].Trim(), parts.Length == 2 ? parts[1].Trim() : fallback);
        }
        return value;
    }
    private static bool Match(string[] parts, int i, List<XElement> nodes, int j)
    {
        if (i < 0)
            return true;
        if (j < 0 || !Simple(parts[i], nodes[j]))
            return false;
        if (i == 0)
            return true;
        if (parts[i - 1] == ">")
            return Match(parts, i - 2, nodes, j - 1);
        for (int k = j - 1; k >= 0; k--)
        if (Match(parts, i - 1, nodes, k))
            return true;
        return false;
    }
    private static bool Simple(string selector, XElement node)
    {
        foreach (Match m in Regex.Matches(selector, @"\[[^\]]+\]|[.#]?[\w-]+|\*"))
        {
            string token = m.Value;
            if (token[0] == '#')
            { if (node.Attribute("id")?.Value != token[1..]) return false; }
            else if (token[0] == '.')
            { if (!(node.Attribute("class")?.Value.Split(' ') ?? []).Contains(token[1..])) return false; }
            else if (token[0] == '[')
            {
                var parts = token[1..^1].Split('=', 2);
                var value = node.Attribute(parts[0])?.Value;
                if (value is null || (parts.Length == 2 && value != parts[1].Trim('"', '\'')))
                    return false;
            }
            else if (token != "*" && token != node.Name.LocalName)
                return false;
        }
        return true;
    }
}
