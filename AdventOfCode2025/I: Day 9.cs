namespace AdventOfCode2025;

public class Day9
{
    public static long LargestRectangle(string input)
    {
        var lines = input
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        List<Tile> tiles = new();
        foreach (var line in lines)
        {
            var parsed = line
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
            
            tiles.Add(new Tile(parsed[0], parsed[1]));
        }

        long largestRectangle = 0;
        Tile? firstCorner = null;
        Tile? secondCorner = null;

        // This could be optimised a lot but is basically instant on the input, so meh
        foreach (var tile in tiles)
        {
            foreach (var otherTile in tiles)
            {
                if (tile.AreaWith(otherTile) > largestRectangle)
                {
                    firstCorner = tile;
                    secondCorner = otherTile;
                    largestRectangle = tile.AreaWith(otherTile);
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