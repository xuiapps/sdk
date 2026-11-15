using System.Globalization;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

// Load-time SVG path parser. Paths are replayed from Path2D, never parsed in Render.
internal ref struct SvgPathReader(ReadOnlySpan<char> text)
{
    private ReadOnlySpan<char> remaining = text;
    public bool More { get { Trim(); return !remaining.IsEmpty; } }
    private void Trim() { while (!remaining.IsEmpty && (char.IsWhiteSpace(remaining[0]) || remaining[0] == ',')) remaining = remaining[1..]; }
    public NFloat Number()
    {
        Trim();
        int i = 0;
        if (i < remaining.Length && remaining[i] is '+' or '-')
            i++;
        while (i < remaining.Length && char.IsDigit(remaining[i]))
            i++;
        if (i < remaining.Length && remaining[i] == '.')
        { i++; while (i < remaining.Length && char.IsDigit(remaining[i])) i++; }
        if (i < remaining.Length && remaining[i] is 'e' or 'E')
        { i++; if (i < remaining.Length && remaining[i] is '+' or '-') i++; while (i < remaining.Length && char.IsDigit(remaining[i])) i++; }
        if (i == 0)
            throw new FormatException("Expected SVG number.");
        var n = (NFloat)double.Parse(remaining[..i], CultureInfo.InvariantCulture);
        remaining = remaining[i..];
        return n;
    }
    private bool Flag() { Trim(); if (remaining.IsEmpty || remaining[0] is not ('0' or '1')) throw new FormatException("Invalid arc flag."); bool flag = remaining[0] == '1'; remaining = remaining[1..]; return flag; }
    public static Path2D Parse(string data)
    {
        var r = new SvgPathReader(data);
        var path = new Path2D();
        Point p = (0, 0), start = p, cubic = p, quadratic = p;
        char command = '\0', previous = '\0';
        while (r.More)
        {
            if (char.IsLetter(r.remaining[0]))
            { command = r.remaining[0]; r.remaining = r.remaining[1..]; }
            bool relative = char.IsLower(command);
            char op = char.ToUpperInvariant(command);
            var origin = relative ? (Vector)p : (Vector)(0, 0);
            Point ReadPoint(ref SvgPathReader reader) => new Point(reader.Number(), reader.Number()) + origin;
            switch (op)
            {
                case 'M':
                    p = ReadPoint(ref r);
                    path.MoveTo(p);
                    start = p;
                    command = relative ? 'l' : 'L';
                    break;
                case 'L':
                    p = ReadPoint(ref r);
                    path.LineTo(p);
                    break;
                case 'H':
                    p = (r.Number() + origin.X, p.Y);
                    path.LineTo(p);
                    break;
                case 'V':
                    p = (p.X, r.Number() + origin.Y);
                    path.LineTo(p);
                    break;
                case 'C':
                    { var a = ReadPoint(ref r); cubic = ReadPoint(ref r); p = ReadPoint(ref r); path.CurveTo(a, cubic, p); break; }
                case 'S':
                    { var a = previous is 'C' or 'S' ? p + (p - cubic) : p; cubic = ReadPoint(ref r); p = ReadPoint(ref r); path.CurveTo(a, cubic, p); break; }
                case 'Q':
                    quadratic = ReadPoint(ref r);
                    p = ReadPoint(ref r);
                    path.CurveTo(quadratic, p);
                    break;
                case 'T':
                    quadratic = previous is 'Q' or 'T' ? p + (p - quadratic) : p;
                    p = ReadPoint(ref r);
                    path.CurveTo(quadratic, p);
                    break;
                case 'A':
                    { var rx = r.Number(); var ry = r.Number(); var angle = r.Number(); var large = r.Flag(); var sweep = r.Flag(); var end = ReadPoint(ref r); Arc(path, p, end, rx, ry, angle, large, sweep); p = end; break; }
                case 'Z':
                    path.ClosePath();
                    p = start;
                    command = '\0';
                    break;
                default:
                    throw new FormatException($"Unsupported SVG path command {command}.");
            }
            previous = op;
        }
        return path;
    }
    private static void Arc(Path2D path, Point from, Point to, NFloat rx, NFloat ry, NFloat degrees, bool large, bool sweep)
    {
        rx = NFloat.Abs(rx);
        ry = NFloat.Abs(ry);
        if (from.X == to.X && from.Y == to.Y)
            return;
        if (rx == 0 || ry == 0)
        { path.LineTo(to); return; }
        var phi = degrees * NFloat.Pi / 180;
        var cos = NFloat.Cos(phi);
        var sin = NFloat.Sin(phi);
        var dx = (from.X - to.X) / 2;
        var dy = (from.Y - to.Y) / 2;
        var x = cos * dx + sin * dy;
        var y = -sin * dx + cos * dy;
        var lambda = x * x / (rx * rx) + y * y / (ry * ry);
        if (lambda > 1)
        { var k = NFloat.Sqrt(lambda); rx *= k; ry *= k; }
        var denominator = rx * rx * y * y + ry * ry * x * x;
        var factor = (large == sweep ? -1 : 1) * NFloat.Sqrt(NFloat.Max(0, (rx * rx * ry * ry - denominator) / denominator));
        var cx = factor * rx * y / ry;
        var cy = -factor * ry * x / rx;
        var center = new Point(cos * cx - sin * cy + (from.X + to.X) / 2, sin * cx + cos * cy + (from.Y + to.Y) / 2);
        var start = NFloat.Atan2((y - cy) / ry, (x - cx) / rx);
        var end = NFloat.Atan2((-y - cy) / ry, (-x - cx) / rx);
        var delta = end - start;
        if (sweep && delta < 0)
            delta += 2 * NFloat.Pi;
        if (!sweep && delta > 0)
            delta -= 2 * NFloat.Pi;
        path.Ellipse(center, rx, ry, phi, start, start + delta, sweep ? Winding.ClockWise : Winding.CounterClockWise);
    }
}
