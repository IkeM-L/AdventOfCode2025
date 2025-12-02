namespace AdventOfCode2025;

public class Day2
{
    private const bool Log = false;

    public static long GetSumRepeats(string input, bool partOne)
    {
        var ranges = input.Split(',');
        long sum = 0;
        foreach (var range in ranges)
        {
            var nums = range.Split('-');
            var start = long.Parse(nums[0]);
            var end = long.Parse(nums[1]);
            if(partOne)
                sum += GetCountTwoRepeatsBetween(start, end);
            else
            {
                sum += GetCountAnyRepeatsBetween(start, end);
            }
        }

        return sum;
    }
    
    private static long GetCountTwoRepeatsBetween(long start, long end)
    {
        long sum = 0;
        for (long i = start; i <= end; i++)
        {
            var iStr = i + "";
            // If the first and second half of the string are equal
            var mid = iStr.Length / 2;
            if (iStr.Length%2 == 0 && iStr[..mid] == iStr[mid..])
            {
                Console.WriteLine(i);
                sum += i;
            }
        }

        if (Log)
        {
            Console.WriteLine($"Range: {start}-{end}: {sum}");
        }

        return sum;
    }

    private static long GetCountAnyRepeatsBetween(long start, long end)
    {
        long sum = 0;
        for (var i = start; i <= end; i++)
        {
            var iStr = i + "";
            foreach (var factor in GetFactors(iStr.Length))
            {
                if(factor == iStr.Length)
                    break;
                
                var tile = iStr[..factor];
                var tiles = true;
                for (int j = 0; j < iStr.Length; j+= factor)
                {
                    if (tile != iStr[j..(j + factor)])
                    {
                        tiles = false;
                        break;
                    }
                }

                if (tiles)
                {
                    sum += i;
                    if (Log)
                    {
                        Console.WriteLine(i);
                    }
                    break;
                }
            }
        }

        if (Log)
        {
            Console.WriteLine($"Range: {start}-{end}: {sum}");
        }

        return sum;
    }

    private static IEnumerable<int> GetFactors(int n)
    {
        if (n == 0) throw new ArgumentException("n must be non-zero.", nameof(n));

        n = Math.Abs(n);

        return Enumerable
            .Range(1, n)
            .Where(d => n % d == 0);
    }
}