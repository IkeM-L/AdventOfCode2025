namespace AdventOfCode2025;

public class Day9
{
    private static Dictionary<long, List<(long startX, long endX)>> _allowedByRow = null!;

    public static long LargestRectangle(string input, bool isPartOne)
    {
        var tiles = input
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray())
            .Select(parsed => new Tile(parsed[0], parsed[1]))
            .ToArray();

        _allowedByRow = !isPartOne ? InitAllowedByRow(tiles) : new Dictionary<long, List<(long startX, long endX)>>();

        long largestRectangle = 0;
        Tile? firstCorner = null;
        Tile? secondCorner = null;

        // This could be optimised a lot but is basically instant on the input, so meh
        foreach (var tile in tiles)
        {
            foreach (var otherTile in tiles)
            {
                var area = tile.AreaWith(otherTile);
                if (area > largestRectangle)
                {
                    if(!isPartOne && !IsRectGreen(tile, otherTile))
                        continue;
                    firstCorner = tile;
                    secondCorner = otherTile;
                    largestRectangle = area;
                }
            }
        }
        
        Console.WriteLine($"{firstCorner}{secondCorner}");

        if (firstCorner is null)
            throw new NullReferenceException($"{nameof(firstCorner)} was not found");
        if (secondCorner is null)
            throw new NullReferenceException($"{nameof(secondCorner)} was not found");
        

        return (Math.Abs(firstCorner.X - secondCorner.X)+1) * (Math.Abs(firstCorner.Y - secondCorner.Y)+1);
    }

    private static Dictionary<long, List<(long startX, long endX)>> InitAllowedByRow(Tile[] tiles)
    {
        var tilesLength = tiles.Length;

        // 1. Precompute edges of the loop.
        var verticalEdges = new List<(long x, long y1, long y2)>();
        var horizontalEdges = new List<(long y, long x1, long x2)>();

        for (var i = 0; i < tilesLength; i++)
        {
            var a = tiles[i];
            var b = tiles[(i + 1) % tilesLength]; // wrap-around

            if (a.X == b.X)
            {
                var x = a.X;
                var y1 = Math.Min(a.Y, b.Y);
                var y2 = Math.Max(a.Y, b.Y);
                if (y1 != y2)
                    verticalEdges.Add((x, y1, y2));
            }
            else if (a.Y == b.Y)
            {
                var y = a.Y;
                var x1 = Math.Min(a.X, b.X);
                var x2 = Math.Max(a.X, b.X);
                if (x1 != x2)
                    horizontalEdges.Add((y, x1, x2));
            }
            else
            {
                throw new InvalidOperationException("Non axis-aligned edge detected; input should only move horizontally/vertically.");
            }
        }

        // 2. For each row, collect x-intersections of vertical edges (scanline interior fill).
        // Using half-open [y1, y2) convention to avoid double-counting vertices.
        var intersectionsPerRow = new Dictionary<long, List<long>>();

        foreach (var (x, y1, y2) in verticalEdges)
        {
            for (var y = y1; y < y2; y++)
            {
                if (!intersectionsPerRow.TryGetValue(y, out var xs))
                {
                    xs = new List<long>();
                    intersectionsPerRow[y] = xs;
                }
                xs.Add(x);
            }
        }

        // 3. Convert intersections into interior intervals per row using even–odd rule.
        var allowedByRow = new Dictionary<long, List<(long startX, long endX)>>();

        foreach (var (y, xs) in intersectionsPerRow)
        {
            xs.Sort();

            var intervals = new List<(long startX, long endX)>();

            for (int i = 0; i + 1 < xs.Count; i += 2)
            {
                var start = xs[i];
                var end = xs[i + 1];

                // We want tiles whose centres are between these crossings, inclusive of boundaries.
                // Interpret as [start, end] in tile coordinates.
                if (start > end)
                {
                    (start, end) = (end, start);
                }

                intervals.Add((start, end));
            }

            if (intervals.Count > 0)
                allowedByRow[y] = intervals;
        }

        // 4. Add horizontal edges as allowed intervals on their exact rows (boundary tiles).
        foreach (var (y, x1, x2) in horizontalEdges)
        {
            if (!allowedByRow.TryGetValue(y, out var intervals))
            {
                intervals = new List<(long startX, long endX)>();
                allowedByRow[y] = intervals;
            }

            intervals.Add((x1, x2));
        }

        // 5. Make sure every red tile itself is allowed (in case some rows have no interior/horiz edges).
        foreach (var t in tiles)
        {
            if (!allowedByRow.TryGetValue(t.Y, out var intervals))
            {
                intervals = new List<(long startX, long endX)>();
                allowedByRow[t.Y] = intervals;
            }

            intervals.Add((t.X, t.X));
        }

        // 6. For each row, merge overlapping / adjacent intervals and normalise.
        foreach (var y in allowedByRow.Keys.ToList())
        {
            var intervals = allowedByRow[y];
            if (intervals.Count == 0)
                continue;

            intervals.Sort((a, b) =>
            {
                var cmp = a.startX.CompareTo(b.startX);
                return cmp != 0 ? cmp : a.endX.CompareTo(b.endX);
            });

            var merged = new List<(long startX, long endX)>();
            var current = intervals[0];

            for (var i = 1; i < intervals.Count; i++)
            {
                var next = intervals[i];
                if (next.startX <= current.endX + 1)
                {
                    // Overlapping or touching; merge
                    current = (current.startX, Math.Max(current.endX, next.endX));
                }
                else
                {
                    merged.Add(current);
                    current = next;
                }
            }

            merged.Add(current);
            allowedByRow[y] = merged;
        }

        return allowedByRow;
    }

    private static bool IsRectGreen(Tile a, Tile b)
    {
        var x1 = (int)Math.Min(a.X, b.X);
        var x2 = (int)Math.Max(a.X, b.X);
        var y1 = (int)Math.Min(a.Y, b.Y);
        var y2 = (int)Math.Max(a.Y, b.Y);

        for (var y = y1; y <= y2; y++)
        {
            if (!_allowedByRow.TryGetValue(y, out var intervals))
                return false;

            // Check if [x1, x2] is fully covered by at least one interval on this row
            var covered = false;
            foreach (var (startX, endX) in intervals)
            {
                if (startX <= x1 && x2 <= endX)
                {
                    covered = true;
                    break;
                }
            }

            if (!covered)
                return false;
        }

        return true;
    }

}

public class Tile(int x, int y) : IEquatable<Tile>
{
    private readonly int _x = x;
    private readonly int _y = y;
    
    public long X => _x;
    public long Y => _y;

    public float DistanceTo(Tile other)
    {
        float dx = _x - other._x;
        float dy = _y - other._y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public long AreaWith(Tile other)
    {
        long width  = Math.Abs(_x - other._x) + 1;
        long height = Math.Abs(_y - other._y) + 1;
        return width * height;
    }
    
    public override string ToString()
    {
        return $"[{_x},{_y}]";
    }

    public bool Equals(Tile? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _x == other._x && _y == other._y;
    }

    public override bool Equals(object? obj) => Equals(obj as Tile);

    public override int GetHashCode() => HashCode.Combine(_x, _y);
}