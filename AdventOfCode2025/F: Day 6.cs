using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day6
{
    public static long GetGrandSum(string input)
    {
        var lines = input.Split(Environment.NewLine);
        var operators = lines.Last(x => x != "");
        operators = operators.Replace(" ", "");
        var output = new long[operators.Length];
        for (var i = 0; i < operators.Length; i++)
        {
            if (operators[i] == '*')
                output[i] = 1;
        }

        foreach (var line in lines)
        {
            var nums = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Check this is a number row
            if(nums.Length == 0 || !long.TryParse(nums[0], out _))
                continue;
            
            for (var i = 0; i < nums.Length; i++)
            {
                if (operators[i] == '+')
                    output[i] += int.Parse(nums[i]);
                if (operators[i] == '*')
                    output[i] *= int.Parse(nums[i]);
            }
        }

        return output.Sum();
    }

    public static long GetGrandColumnSum(string input)
    {
        var lines = input.Split(Environment.NewLine);

        // strip empty lines at end, then take last non-empty as operator line
        var nonEmptyLines = lines.Where(l => l.Length > 0).ToList();
        if (nonEmptyLines.Count == 0)
            return 0;

        var operatorsLine = nonEmptyLines[^1];

        // positions and characters of non-space operators in that line
        var opPositions = operatorsLine
            .Select((ch, i) => (ch, i))
            .Where(t => t.ch != ' ')
            .ToArray();

        var opChars = opPositions.Select(t => t.ch).ToArray();

        var calculations = new ColumnSumHelper?[opChars.Length];

        // process all lines except the operator line itself
        foreach (var line in nonEmptyLines.Take(nonEmptyLines.Count - 1))
        {
            for (var i = 0; i < opPositions.Length; i++)
            {
                var start = opPositions[i].i;
                var end   = (i + 1 < opPositions.Length) ? opPositions[i + 1].i : line.Length;

                if (start >= line.Length)
                    continue; // nothing in this line for this column

                int length = Math.Max(0, Math.Min(end, line.Length) - start);
                if (length <= 0)
                    continue;

                string segment = line.Substring(start, length);

                calculations[i] ??= new ColumnSumHelper(opChars[i], segment.Length);

                calculations[i].AddNumberLine(segment);
            }
        }

        return calculations.Where(x => x != null).Sum(x => x.GetTotal());
    }

}

internal class ColumnSumHelper(char opChar, int numberOfNumbers)
{
    private readonly long[] _numbers = new long[numberOfNumbers];

    public long GetTotal()
    {
        if (opChar == '+')
            return _numbers.Sum();
        if (opChar == '*')
        {
            return _numbers.Where(number => number != 0).Aggregate(1L, (current, number) => current * number);
        }

        throw new ArgumentException("Unknown operator");
    }

    public void AddNumberLine(string line)
    {
        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character != ' ')
            {
                _numbers[i] *= 10;
                _numbers[i] += character - '0';
            }
        }
    }
}