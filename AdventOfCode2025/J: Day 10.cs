using System.Collections;
using System.Text.RegularExpressions;

namespace AdventOfCode2025;

public class Day10
{
    private const bool Log = true;
    
    public static int GetMinimumNumPresses(string input, bool isPartOne)
    {
        var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        int sum = 0;

        foreach (var line in lines)
        {
            var match = Regex.Match(
                line,
                @"^\[(?<pattern>[^\]]+)\]\s+(?<groups>(\([^)]+\)\s*)+)\{(?<tail>[^}]+)\}$"
            );

            if (!match.Success)
            {
                // Depending on how strict you want to be:
                // throw, continue, or handle differently
                continue;
            }

            // pattern string -> bool[] (true == '#', false == '.')
            var desiredButtonConfig = match
                .Groups["pattern"]
                .Value
                .Select(c => c == '#')
                .ToArray();

            var buttons = Regex.Matches(match.Groups["groups"].Value, @"\(([^)]+)\)")
                .Select(m => m.Groups[1].Value.Split(',').Select(int.Parse).ToList())
                .ToList();

            // not needed for part 1, but we may as well parse it
            var joltages = match.Groups["tail"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var enumerator = new EnumerableButtonStates(buttons, desiredButtonConfig.Length);

            foreach (var (numPresses, buttonState) in enumerator)
            {
                if (buttonState.SequenceEqual(desiredButtonConfig))
                {
                    sum += numPresses;
                    if(Log)
                        Console.WriteLine(numPresses);
                    break;
                }
                // Safety
                if(numPresses > 100)
                {
                    if(Log)
                        Console.WriteLine("We are too deep! Bailing");
                    break;
                }

                if(Log)
                {
                    var buttonLog = new string(buttonState.Select(x => x ? '#' : '.').ToArray());
                    var expectedButtons = new string(desiredButtonConfig.Select(x => x ? '#' : '.').ToArray());
                    Console.WriteLine($"{numPresses}, {buttonLog}, {expectedButtons}");
                }
            }
        }

        return sum;
    }
}

/// <summary>
/// BFS over button states: start from all '.' (all false), then explore
/// all states reachable by 1 press, then 2 presses, etc.
/// Each edge corresponds to “press one of the available buttons once”.
/// </summary>
public class EnumerableButtonStates : IEnumerable<(int numPresses, bool[] buttonState)>
{
    private readonly List<List<int>> _buttons;
    private readonly int _outputLength;

    public EnumerableButtonStates(List<List<int>> buttons, int outputLength)
    {
        _buttons = buttons;
        _outputLength = outputLength;
    }

    public IEnumerator<(int numPresses, bool[] buttonState)> GetEnumerator()
    {
        // Initial state: all '.' (all false)
        var startState = new bool[_outputLength];

        // Queue for BFS: (currentState, depth == number of presses)
        var queue = new Queue<(bool[] state, int depth)>();
        queue.Enqueue((startState, 0));
        
        var visited = new HashSet<string>();
        visited.Add(StateKey(startState));

        while (queue.Count > 0)
        {
            var (state, depth) = queue.Dequeue();

            // Yield current state
            yield return (depth, state);

            // Generate neighbours by pressing each button once
            foreach (var button in _buttons)
            {
                var nextState = PressButtons(state, button);
                
                var key = StateKey(nextState);
                if (!visited.Add(key))
                    continue;

                queue.Enqueue((nextState, depth + 1));
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static bool[] PressButtons(bool[] input, List<int> buttons)
    {
        var output = (bool[])input.Clone();
        foreach (var button in buttons)
        {
            output[button] = !output[button];
        }

        return output;
    }
    
    private static string StateKey(bool[] state)
    {
        // e.g. "#.#.." style
        var chars = new char[state.Length];
        for (int i = 0; i < state.Length; i++)
        {
            chars[i] = state[i] ? '#' : '.';
        }
        return new string(chars);
    }
}
