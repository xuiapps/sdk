using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Runtime.Software.Tessellate;

public partial class PathTesselator
{
    public partial class ScanlineSweep
    {
        public static List<Polygon> Fill(IReadOnlyList<ClosedContour> contours, FillRule fillRule)
        {
            var triangles = new List<Polygon>();
            SweepAndEmitTrapezoids(contours, fillRule, triangles);
            return triangles;
        }

        public static void SweepAndEmitTrapezoids(
            IReadOnlyList<ClosedContour> contours,
            FillRule fillRule,
            List<Polygon> outputTriangles)
        {
            var contourEdges = IntersectAndSplitContours(contours);
            // var nonIntersectingLoops = ExtractLoops(contourEdges);
            // var edges = ContoursToEdges(nonIntersectingLoops);
            var edges = contourEdges;

            var yEvents = new SortedSet<nfloat>();
            foreach (var e in edges)
            {
                yEvents.Add(e.YMin);
                yEvents.Add(e.YMax);
            }

            var yList = new List<nfloat>(yEvents);
            var active = new List<Edge>();

            for (int i = 0; i < yList.Count - 1; i++)
            {
                nfloat y0 = yList[i];
                nfloat y1 = yList[i + 1];

                active.RemoveAll(e => e.YMax <= y0);

                foreach (var e in edges)
                {
                    if (e.YMin == y0 && e.YMax > y0)
                        active.Add(e);
                }

                // At a shared vertex both edges have the same X. Order within the
                // open band, and never add horizontal edges (their inverse slope is infinite).
                nfloat middle = (y0 + y1) / 2;
                active.Sort((a, b) => a.XAt(middle).CompareTo(b.XAt(middle)));

                int winding = 0;
                for (int j = 0; j < active.Count - 1; j++)
                {
                    var left = active[j];
                    var right = active[j + 1];

                    winding += left.Winding;

                    bool shouldFill = fillRule switch
                    {
                        FillRule.EvenOdd => (winding % 2) != 0,
                        FillRule.NonZero => winding != 0,
                        _ => false
                    };

                    if (shouldFill)
                    {
                        var trapezoid = TrapezoidFromEdges(left, right, y0, y1);
                        outputTriangles.Add(trapezoid.Polygon1);
                        outputTriangles.Add(trapezoid.Polygon2);
                    }
                }
            }
        }

        private static List<Edge> IntersectAndSplitContours(IReadOnlyList<ClosedContour> contours)
        {
            var segments = new List<(Point A, Point B)>();
            foreach (var contour in contours)
            {
                for (int i = 0; i < contour.Count; i++)
                {
                    var a = contour[i];
                    var b = contour[(i + 1) % contour.Count];
                    if (a != b)
                        segments.Add((a, b));
                }
            }

            var splitPoints = new Dictionary<(Point, Point), List<Point>>();

            // TODO: O^2 complexity
            for (int i = 0; i < segments.Count; i++)
            {
                var (a1, b1) = segments[i];
                for (int j = i + 1; j < segments.Count; j++)
                {
                    var (a2, b2) = segments[j];
                    if (TryIntersectSegments(a1, b1, a2, b2, out var p))
                    {
                        if (!splitPoints.ContainsKey((a1, b1))) splitPoints[(a1, b1)] = new();
                        if (!splitPoints.ContainsKey((a2, b2))) splitPoints[(a2, b2)] = new();
                        splitPoints[(a1, b1)].Add(p);
                        splitPoints[(a2, b2)].Add(p);
                    }
                }
            }

            var splitEdges = new List<Edge>();

            foreach (var (a, b) in segments)
            {
                var list = new List<Point> { a, b };
                if (splitPoints.TryGetValue((a, b), out var splits))
                {
                    list.AddRange(splits);
                }
                list = list.Distinct().OrderBy(p => (p - a).MagnitudeSquared).ToList();
                for (int i = 0; i < list.Count - 1; i++)
                {
                    if (list[i] != list[i + 1])
                        splitEdges.Add(new Edge(list[i], list[i + 1]));
                }
            }

            return splitEdges;
        }

        private static bool TryIntersectSegments(Point a, Point b, Point c, Point d, out Point intersection)
        {
            var r = b - a;
            var s = d - c;
            var denom = Xui.Core.Math2D.Vector.Cross(r, s);
            var delta = c - a;

            if (nfloat.Abs(denom) < 1e-6f)
            {
                intersection = default;
                return false;
            }

            var t = Xui.Core.Math2D.Vector.Cross(delta, s) / denom;
            var u = Xui.Core.Math2D.Vector.Cross(delta, r) / denom;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                intersection = a + t * r;
                return true;
            }

            intersection = default;
            return false;
        }

        /// <summary>
        /// Extracts all closed, non-intersecting loops from a set of directed edges.
        /// Each loop is returned as a ClosedContour.
        /// </summary>
        public static List<PathTesselator.ClosedContour> ExtractLoops(List<PathTesselator.ScanlineSweep.Edge> edges)
        {
            var adjacency = new Dictionary<Point, List<Point>>();
            var visited = new HashSet<(Point A, Point B)>();
            var loops = new List<PathTesselator.ClosedContour>();

            foreach (var edge in edges)
            {
                if (!adjacency.TryGetValue(edge.Start, out var list))
                    adjacency[edge.Start] = list = new();

                list.Add(edge.End);
            }

            foreach (var start in adjacency.Keys)
            {
                foreach (var next in adjacency[start])
                {
                    if (visited.Contains((start, next)))
                        continue;

                    var loop = new List<Point> { start };
                    var current = next;
                    var prev = start;

                    while (true)
                    {
                        visited.Add((prev, current));
                        loop.Add(current);

                        if (!adjacency.TryGetValue(current, out var nextPoints))
                            break;

                        var found = false;
                        foreach (var candidate in nextPoints)
                        {
                            if (!visited.Contains((current, candidate)))
                            {
                                prev = current;
                                current = candidate;
                                found = true;
                                break;
                            }
                        }

                        if (!found || current == loop[0])
                            break;
                    }

                    if (loop.Count >= 4 && loop[0] == loop[^1]) // at least 3 distinct points + closing
                    {
                        loop.RemoveAt(loop.Count - 1); // Remove duplicate closing point
                        loops.Add(new PathTesselator.ClosedContour(loop));
                    }
                }
            }

            return loops;
        }

        private static List<Edge> ContoursToEdges(IReadOnlyList<PathTesselator.ClosedContour> contours)
        {
            var edges = new List<Edge>();

            foreach (var contour in contours)
            {
                for (int i = 0; i < contour.Count; i++)
                {
                    var a = contour[i];
                    var b = contour[(i + 1) % contour.Count];
                    if (a != b)
                        edges.Add(new Edge(a, b));
                }
            }

            return edges;
        }

        private static Trapezoid TrapezoidFromEdges(Edge left, Edge right, nfloat y0, nfloat y1)
        {
            var a = new Point(left.XAt(y0), y0);
            var b = new Point(right.XAt(y0), y0);
            var c = new Point(right.XAt(y1), y1);
            var d = new Point(left.XAt(y1), y1);
            return new Trapezoid(a, b, c, d);
        }

        public readonly struct Edge
        {
            public readonly Point Start;
            public readonly Point End;
            public readonly nfloat XAtYMin;
            public readonly nfloat SlopeInv;

            public readonly nfloat YMin;
            public readonly nfloat YMax;
            public readonly int Winding;

            public Edge(Point a, Point b)
            {
                // Preserve original contour direction before normalizing endpoints by Y.
                Winding = b.Y > a.Y ? 1 : -1;
                if (a.Y < b.Y)
                {
                    Start = a;
                    End = b;
                }
                else
                {
                    Start = b;
                    End = a;
                }

                YMin = Start.Y;
                YMax = End.Y;
                XAtYMin = Start.X;
                SlopeInv = (End.X - Start.X) / (End.Y - Start.Y);
            }

            public nfloat XAt(nfloat y)
            {
                return XAtYMin + SlopeInv * (y - YMin);
            }
        }
    }
}
