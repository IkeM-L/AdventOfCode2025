namespace AdventOfCode2025;

public class Day3
{
    public static long FindTotalJoltage(string input, bool isPartOne)
    {
        long sum = 0;
        var banks = input.Split('\n');
        foreach (var bank in banks)
        {
            if (bank == "")
            {
                continue;
            }
            if(isPartOne)
                sum += FindMaximumJoltage(bank);
            else
            {
                sum += FindMaximumJoltage(bank, 12);
            }
        }


        return sum;
    }

    private static int FindMaximumJoltage(string bank)
    {
        var largest = 0;
        var largestAfter = 0;
        for (var index = 0; index < bank.Length; index++)
        {
            if (int.TryParse(bank[index] + "", out var cur))
            {
                if (cur > largest && index != bank.Length - 1)
                {
                    largest = cur;
                    // reset largest after
                    largestAfter = 0;
                }
                else if (cur > largestAfter)
                {
                    largestAfter = cur;
                }
            }
        }

        return (largest * 10) + largestAfter;
    }
    
    private static long FindMaximumJoltage(string bank, long numBatteries)
    {
        long[] largests = new long[numBatteries];
        
        for (var index = 0; index < bank.Length; index++)
        {
            if (!long.TryParse(bank[index] + "", out var cur))
            {
                Console.WriteLine($"Skipping!");
                continue;
            }

            var set = false;
            // iterate over the largest number
            for (var i = 0; i < largests.Length; i++)
            {
                // If it isn't larger, we have used this battery, or it is too late to finish our n digit number
                if(cur <= largests[i] || set || bank.Length - index < largests.Length - i)
                    continue;

                // Zero remaining largests
                largests[i] = cur;
                for (int j = i + 1; j < largests.Length; j++)
                {
                    largests[j] = 0;
                }
                set = true;
            }
        }
        
        long total = 0;
        for (long i = 0; i < largests.Length; i++)
        {
            total *= 10;
            total += largests[i];
        }

        return total;
    }
}