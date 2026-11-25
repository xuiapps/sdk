using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Xui.Compiler.WASM.Wasm.Section;

namespace Xui.Compiler.Tests.Wasm.TestApp;

/// <summary>Accounts for the actual UTF-8 bytes of the generated, self-contained preview.</summary>
internal static class PageSizeBreakdown
{
    internal const string Placeholder = "__PAGE_SIZE_BREAKDOWN__";

    internal static string AddTo(string html, byte[] wasm)
    {
        int wasmText = Convert.ToBase64String(wasm).Length;
        var fonts = Regex.Matches(html, @"font-weight:(\d+); src:url\(data:font/ttf;base64,([^)]*)\)")
            .Select(m => (Weight: m.Groups[1].Value, Raw: Convert.FromBase64String(m.Groups[2].Value).Length, Encoded: m.Groups[2].Length)).ToArray();
        int fontText = fonts.Sum(f => f.Encoded);
        int javascript = Regex.Matches(html, @"<script>([\s\S]*?)</script>")
            .Sum(m => Encoding.UTF8.GetByteCount(m.Groups[1].Value)) - wasmText;
        int envelope = Encoding.UTF8.GetByteCount(html) - Placeholder.Length;
        var sections = new StringBuilder();
        sections.Append(SectionRow("Header", 8, "Magic bytes and module version"));
        int position = 8;
        while (position < wasm.Length)
        {
            int start = position;
            var id = (SectionId)wasm[position++];
            uint length = 0;
            int shift = 0;
            byte next;
            do { next = wasm[position++]; length |= (uint)(next & 127) << shift; shift += 7; } while ((next & 128) != 0);
            position += checked((int)length);
            sections.Append(SectionRow(id.ToString(), position - start, id switch
            {
                SectionId.Type => "Function signatures and GC object/array shapes",
                SectionId.Import => "JavaScript and JS-string host bindings",
                SectionId.Function => "Function-to-signature index map",
                SectionId.Table => "Indirect-call table definitions",
                SectionId.Memory => "Linear-memory limits (not allocated memory contents)",
                SectionId.Tag => "Managed exception tag definitions",
                SectionId.Global => "Globals and initializers, including runtime metadata",
                SectionId.Export => "Names exposed to JavaScript",
                SectionId.Element => "Function references for indirect calls",
                SectionId.Code => "Compiled methods and compiler runtime helper bodies",
                SectionId.DataCount => "Number of data segments",
                SectionId.Data => "String literals and other embedded constant data",
                SectionId.Start => "Module start function index",
                _ => "Custom module metadata"
            }));
        }

        int total = envelope;
        while (true)
        {
            int markup = total - wasmText - fontText - javascript;
            string panel = $"""
                <details id="page-size">
                  <summary>Page size · {Bytes(total)} bytes</summary>
                  <h2>Standalone HTML</h2>
                  <p>Exact UTF-8 file bytes, before compression. Binary assets are base64-encoded inside this file; the extra bytes are encoding overhead.</p>
                  <table id="html-size"><thead><tr><th>Content</th><th>Raw bytes</th><th>HTML bytes</th><th>Share</th></tr></thead><tbody>
                  {Row("WASM module", wasm.Length, wasmText, total)}
                  {string.Join("\n", fonts.Select(f => Row("Inter font · " + f.Weight, f.Raw, f.Encoded, total)))}
                  {Row("JavaScript · runtime, Canvas, input and bootstrap", javascript, javascript, total)}
                  {Row("HTML, CSS and metadata", markup, markup, total)}
                  {Row("Image resources", 0, 0, total)}
                  </tbody><tfoot><tr><th colspan="2">Total file</th><td data-total="{total}">{Bytes(total)}</td><td>100%</td></tr></tfoot></table>
                  <p>No external assets or image files. The logo, phone frame and icons are drawn through Canvas by compiled Xui code. Inter's four font files are embedded above.</p>
                  <h2>Inside the WASM module</h2>
                  <p>These bytes are already included in the WASM row above. Section sizes include their ID and length prefix.</p>
                  <table id="wasm-size"><thead><tr><th>Section</th><th>Bytes</th><th>Contents</th></tr></thead><tbody>{sections}</tbody><tfoot><tr><th>Total binary</th><td>{Bytes(wasm.Length)}</td><td></td></tr></tfoot></table>
                  <p>WASM contains the reachable TestApp, Xui framework and compiler runtime code. Roslyn, input DLLs and the .NET browser runtime are not shipped. Runtime memory, GC objects and Canvas pixel buffers are not file bytes.</p>
                </details>
                """;
            int actual = envelope + Encoding.UTF8.GetByteCount(panel);
            if (actual == total) return html.Replace(Placeholder, panel, StringComparison.Ordinal);
            total = actual; // Include the accounting panel itself in the final total.
        }
    }

    private static string Bytes(int count) => count.ToString("N0", CultureInfo.InvariantCulture);
    private static string Row(string name, int raw, int encoded, int total) =>
        $"<tr data-bytes=\"{encoded}\"><td>{WebUtility.HtmlEncode(name)}</td><td>{Bytes(raw)}</td><td>{Bytes(encoded)}</td><td>{(100d * encoded / total).ToString("F1", CultureInfo.InvariantCulture)}%</td></tr>";
    private static string SectionRow(string name, int count, string description) =>
        $"<tr data-bytes=\"{count}\"><td>{name}</td><td>{Bytes(count)}</td><td>{description}</td></tr>";
}
