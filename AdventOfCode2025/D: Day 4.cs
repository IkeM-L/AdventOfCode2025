using System.Text;

namespace AdventOfCode2025;

public class Day4
{
    private const bool Log = false;
    
    public static long FindTotalRolls(string input, bool isPartOne)
    {
        if(isPartOne)
        {
            var sum = GetNumMovable(input, out var after);
            if (Log)
            {
                Console.WriteLine(after);
            }

            return sum;
        }
        else
        {
            long sum = 0;
            long newSum;
            do
            {
                newSum = GetNumMovable(input, out var after);
                if (Log)
                {
                    Console.WriteLine(after);
                }

                input = after;
                sum += newSum;
            } while (newSum != 0);

            return sum;
        }
    }

    private static long GetNumMovable(string input, out string after)
    {
        long sum = 0;
        StringBuilder afterBuilder = new StringBuilder();
        var lines = input.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            // Very minimal input validation in case blank lines were pasted. We expect such lines 
            // to be at the end, other than that we assume input is valid as we do generally
            if(line.Length == 0)
                continue;
            
            for (int j = 0; j < line.Length; j++)
            {
                int neighbours = 0;
                if(line[j] != '@')
                {
                    afterBuilder.Append('.');
                    continue;
                }
                
                if (i > 0 && lines[i - 1][j] == '@')   // N
                    neighbours++;
                
                if (j > 0 && lines[i][j - 1] == '@') // W
                    neighbours++;
                
                if (i < lines.Length - 1 && lines[i + 1][j] == '@') // S
                    neighbours++;
                
                if (j < lines[i].Length - 1 && lines[i][j + 1] == '@') // W
                    neighbours++;
                
                if (i > 0 && j < lines[i].Length - 1 && lines[i - 1][j + 1] == '@') //NE
                    neighbours++;
                
                if (i > 0 && j > 0 && lines[i - 1][j - 1] == '@') // NW
                    neighbours++;
                
                if (i < lines.Length - 1 && j > 0 && lines[i + 1][j - 1] == '@') // SW
                    neighbours++;
                
                if (i < lines.Length - 1 && j < lines[i].Length - 1 && lines[i + 1][j + 1] == '@') // SE
                    neighbours++;

                if (neighbours < 4)
                {
                    sum++;
                    afterBuilder.Append('x');
                }
                else
                {
                    afterBuilder.Append('@');
                }
            }
            afterBuilder.AppendLine();
        }

        after = afterBuilder.ToString().Trim();
        return sum;
    }
}