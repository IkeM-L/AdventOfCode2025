using System.Collections;
using System.Text.RegularExpressions;
using Google.OrTools.Sat;

namespace AdventOfCode2025;

public class Day10
{
    private const bool Log = false;

    public static int GetMinimumNumPresses(string input, bool isPartOne)
    {
        var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var sum = 0;

        foreach (var line in lines)
        {
            var match = Regex.Match(
                line,
                @"^\[(?<pattern>[^\]]+)\]\s+(?<groups>(\([^)]+\)\s*)+)\{(?<tail>[^}]+)\}$"
            );

            if (!match.Success)
                // Depending on how strict you want to be:
                // throw, continue, or handle differently
                continue;

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
            var expectedJoltages = match.Groups["tail"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var enumerator = new EnumerableButtonStates(buttons, desiredButtonConfig.Length, expectedJoltages.Count);

            if (isPartOne)
            {
                foreach (var (numPresses, buttonState) in enumerator)
                {
                    if (buttonState.SequenceEqual(desiredButtonConfig))
                    {
                        sum += numPresses;
                        if (Log)
                            Console.WriteLine(numPresses);
                        break;
                    }

                    // Safety
                    if (numPresses > 100)
                    {
                        if (Log)
                            Console.WriteLine("We are too deep! Bailing");
                        break;
                    }

                    if (Log)
                    {
                        var buttonLog = new string(buttonState.Select(x => x ? '#' : '.').ToArray());
                        var expectedButtons = new string(desiredButtonConfig.Select(x => x ? '#' : '.').ToArray());
                        Console.WriteLine($"{numPresses}, {buttonLog}, {expectedButtons}");
                    }
                }
            }
            else
            {
                // Part 2 implemented by GPT according to my idea because
                // I don't have time for a full LE 
                // solver or learning a SAT library today! 
                var intTarget = expectedJoltages.ToArray();
                var presses = SolveMachineWithOrTools(buttons, intTarget);
                sum += presses;
                if(Log)
                    Console.WriteLine(presses);
            }
        }

        return sum;
    }

    private static int SolveMachineWithOrTools(
        List<List<int>> buttons,
        int[] target)
    {
        int n = target.Length;
        int m = buttons.Count;

        // Build B[i,j]
        int[,] B = new int[n, m];
        for (int j = 0; j < m; j++)
        {
            foreach (var idx in buttons[j])
                B[idx, j] += 1;
        }

        // Compute per-variable upper bounds
        int[] maxX = new int[m];
        for (int j = 0; j < m; j++)
        {
            int ub = int.MaxValue;
            bool any = false;
            for (int i = 0; i < n; i++)
            {
                if (B[i, j] > 0)
                {
                    any = true;
                    ub = Math.Min(ub, target[i] / B[i, j]); // B[i,j] is 0 or 1
                }
            }
            maxX[j] = any ? ub : 0;
        }

        var model = new CpModel();

        // Decision variables
        IntVar[] x = new IntVar[m];
        for (int j = 0; j < m; j++)
        {
            x[j] = model.NewIntVar(0, maxX[j], $"x_{j}");
        }

        // Counter constraints
        for (int i = 0; i < n; i++)
        {
            var terms = new List<LinearExpr>();
            for (int j = 0; j < m; j++)
            {
                if (B[i, j] != 0)
                    terms.Add(B[i, j] * x[j]);
            }

            model.Add(LinearExpr.Sum(terms) == target[i]);
        }

        // Objective: minimise total presses
        model.Minimize(LinearExpr.Sum(x));

        // Solver + parameters
        var solver = new CpSolver();
        
        var status = solver.Solve(model);

        if (status != CpSolverStatus.Optimal && status != CpSolverStatus.Feasible)
            throw new InvalidOperationException($"Solver failed: {status}");

        int result = 0;
        for (int j = 0; j < m; j++)
            result += (int)solver.Value(x[j]);

        return result;
    }


}

/// <summary>
///     BFS over button states: start from all '.' (all false), then explore
///     all states reachable by 1 press, then 2 presses, etc.
///     Each edge corresponds to “press one of the available buttons once”.
/// </summary>
public class EnumerableButtonStates : IEnumerable<(int numPresses, bool[] buttonState)>
{
    private readonly List<List<int>> _buttons;
    private readonly int _joltageLength;
    private readonly int _outputLength;

    public EnumerableButtonStates(List<List<int>> buttons, int outputLength, int joltageLength)
    {
        _buttons = buttons;
        _outputLength = outputLength;
        _joltageLength = joltageLength;
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private int[] PressJoltageButtons(int[] input, List<int> buttons)
    {
        var output = (int[])input.Clone();
        foreach (var button in buttons) output[button]++;

        return output;
    }


    private static bool[] PressButtons(bool[] input, List<int> buttons)
    {
        var output = (bool[])input.Clone();
        foreach (var button in buttons) output[button] = !output[button];

        return output;
    }

    private static string StateKey(bool[] state)
    {
        // e.g. "#.#.." style
        var chars = new char[state.Length];
        for (var i = 0; i < state.Length; i++) chars[i] = state[i] ? '#' : '.';
        return new string(chars);
    }
}