namespace AdventOfCode2025;

public class Day5
{
    public static long CountNumFreshIngredients(string input, bool isPartOne)
    {
        var sum = 0;
        var lines = input.Split('\n');
        var freshChecker = new FreshCheck();
        foreach (var line in lines)
        {
            if (line.Contains('-'))
            {
                var lower = long.Parse(line.Split('-')[0]);
                var upper = long.Parse(line.Split('-')[1]);
                freshChecker.AddRange(lower, upper);
            }
            else if(line.Length != 0 && isPartOne)
            {
                if (freshChecker.IsInRange(long.Parse(line)))
                {
                    sum++;
                }
            }
            else
            {
                return freshChecker.GetNumFresh();
            }
        }

        return sum;
    }
}

public class FreshCheck
{
    private readonly List<Range> _ranges = new();
    private bool _normalised;

    public void AddRange(long lower, long upper)
    {
        _ranges.Add(new Range(lower, upper));
        _normalised = false;
    }

    private void EnsureNormalised()
    {
        if (_normalised || _ranges.Count <= 1)
            return;

        // Sort by Lower then Upper
        _ranges.Sort();

        var merged = new List<Range>();
        var current = _ranges[0];

        for (int i = 1; i < _ranges.Count; i++)
        {
            var next = _ranges[i];

            // If ranges overlap or touch, merge them
            if (next.Lower <= current.Upper + 1)
            {
                current = new Range(
                    current.Lower,
                    Math.Max(current.Upper, next.Upper));
            }
            else
            {
                merged.Add(current);
                current = next;
            }
        }

        merged.Add(current);

        _ranges.Clear();
        _ranges.AddRange(merged);

        _normalised = true;
    }

    public bool IsInRange(long value)
    {
        if (_ranges.Count == 0)
            return false;

        EnsureNormalised();

        int lo = 0;
        int hi = _ranges.Count - 1;

        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) / 2);
            var range = _ranges[mid];

            if (value < range.Lower)
            {
                hi = mid - 1;
            }
            else if (value > range.Upper)
            {
                lo = mid + 1;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public long GetNumFresh()
    {
        if (_ranges.Count == 0)
            return 0;

        EnsureNormalised();

        return _ranges.Sum(range => range.Upper - range.Lower + 1);
    }
}


public sealed class Range : IComparable<Range>
{
    public long Lower { get; }
    public long Upper { get; }

    public Range(long lower, long upper)
    {
        if (lower > upper)
            throw new ArgumentException("Lower bound must be <= upper bound.", nameof(lower));

        Lower = lower;
        Upper = upper;
    }

    public int CompareTo(Range? other)
    {
        if (other is null)
            return 1;

        int cmp = Lower.CompareTo(other.Lower);
        if (cmp != 0)
            return cmp;

        return Upper.CompareTo(other.Upper);
    }
}