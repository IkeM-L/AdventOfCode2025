using AdventOfCode2025;

const int day = 10;
const PuzzleInput inputType = PuzzleInput.Actual;
const PuzzlePart part = PuzzlePart.Two;

var input = GetInputFromFile(day, inputType);

switch (day)
{
    case 1:
        Console.WriteLine(Day1.GetNumZeros(input, (part == PuzzlePart.Two ? true : false)));
        break;
    case 2:
        Console.WriteLine(Day2.GetSumRepeats(input, part == PuzzlePart.One));
        break;
    case 3:
        Console.WriteLine(Day3.FindTotalJoltage(input, part == PuzzlePart.One));
        break;
    case 4:
        Console.WriteLine(Day4.FindTotalRolls(input, part == PuzzlePart.One));
        break;
    case 5:
        Console.WriteLine(Day5.CountNumFreshIngredients(input, part == PuzzlePart.One));
        break;
    case 6:
        Console.WriteLine(part == PuzzlePart.One ? Day6.GetGrandSum(input) : Day6.GetGrandColumnSum(input));
        break;
    case 7:
        Console.WriteLine(part == PuzzlePart.One ? Day7.GetNumSplits(input) : Day7.GetNumTimelines(input));
        break;
    case 8:
        Console.WriteLine(Day8.Solve(input, (inputType == PuzzleInput.Actual ? 1000 : 10), part == PuzzlePart.One));
        break;
    case 9:
        Console.WriteLine(Day9.LargestRectangle(input, part == PuzzlePart.One));
        break;
    case 10:
        Console.WriteLine(Day10.GetMinimumNumPresses(input, part == PuzzlePart.One));
        break;
    default:
        break;
}


string GetInputFromFile(int i, PuzzleInput puzzleInput)
{
    string path = $"../../../Day{i}{(puzzleInput == PuzzleInput.Example? "Example" : "") }.txt";
    return File.ReadAllText(path);
}