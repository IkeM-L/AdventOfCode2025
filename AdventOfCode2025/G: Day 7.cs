namespace AdventOfCode2025;

public class Day7
{
    public static long GetNumSplits(string input)
    {
        long numSplits = 0;
        var lines = input.Split(Environment.NewLine);
        bool[] isBeam = new bool[lines[0].Length];
        isBeam[lines[0].IndexOf('S')] = true;
        foreach (var line in lines)
        {
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == '^' && isBeam[i])
                {
                    isBeam[i] = false;
                    numSplits++;
                    if (i != 0) 
                        isBeam[i - 1] = true;

                    if (i != lines[0].Length - 1) 
                        isBeam[i + 1] = true;
                }
            }
        }

        return numSplits;
    }

    public static long GetNumTimelines(string input)
    {
        var lines = input.Split(Environment.NewLine);
        long[] isBeam = new long[lines[0].Length];
        isBeam[lines[0].IndexOf('S')] = 1;
        foreach (var line in lines)
        {
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == '^' && isBeam[i] > 0)
                {
                    if (i != 0) 
                        isBeam[i - 1] += isBeam[i];

                    if (i != lines[0].Length - 1) 
                        isBeam[i + 1] += isBeam[i];
                    
                    isBeam[i] = 0;
                }
            }
        }
        return isBeam.Sum();
    }
}