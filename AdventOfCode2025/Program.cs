using AdventOfCode2025;

const int day = 1;
const PuzzleInput inputType = PuzzleInput.Actual;
const PuzzlePart part = PuzzlePart.Two;

var input = GetInputFromFile(day, inputType);

switch (day)
{
    case 1:
        Console.WriteLine(Day1.GetNumZeros(input, (part == PuzzlePart.Two ? true : false)));
        break;
    default:
        break;
}


string GetInputFromFile(int i, PuzzleInput puzzleInput)
{
    string path = $"../../../Day{i}{(puzzleInput == PuzzleInput.Example? "Example" : "") }.txt";
    return File.ReadAllText(path);
}