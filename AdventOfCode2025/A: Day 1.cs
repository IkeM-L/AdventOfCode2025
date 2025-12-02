namespace AdventOfCode2025;

public class Day1
{
    private const bool Log = false;
    public static int GetNumZeros(string input, bool countAllZeros)
    {
        var turns = input.Split('\n');
        var dial = new Dial(99, countAllZeros, Log);
        foreach (var turn in turns)
        {
            if(turn == "")
                continue;
            
            if (turn[0] == 'L')
            {
                dial.Left(int.Parse(turn.Substring(1)));
            }
            if (turn[0] == 'R')
            {
                dial.Right(int.Parse(turn.Substring(1)));
            }
        }

        return dial.NumZeros;
    }
}

public class Dial
{
    private readonly int _size;   // number of positions = maxValue + 1
    private int _value = 50;           // current position 0.._size-1
    private int _numZeros;    // start at zero, count that

    private readonly bool _countAllZeros;
    private readonly bool _log;

    public Dial(int maxValue, bool countAllZeros, bool log = false)
    {
        _size = maxValue + 1;
        _log = log;
        _countAllZeros = countAllZeros;
        Console.WriteLine($"CountAllZeros: {countAllZeros}");
    }

    public int NumZeros => _numZeros;

    public void Right(int amount)
    {
        if (amount < 0)
        {
            Left(-amount);
            return;
        }
        
        int partial = amount % _size;
        int next = _value + partial;
        if (_countAllZeros)
        {
            _numZeros += (amount + _value) / _size;
        }
        
        _value = next % _size;
        if (_value == 0 && !_countAllZeros)
            _numZeros++;
        

        if (_log)
        {
            Console.WriteLine("Current value: {0}, current zeros {1}", _value, _numZeros);
        }
    }

    public void Left(int amount)
    {
        if (amount < 0)
        {
            Right(-amount);
            return;
        }

        amount = amount % (_size * 1_000_000_000); // avoid overflow in pathological inputs, optional

        int partial = amount % _size;
        int next = _value - partial;

        if (_countAllZeros && amount > 0)
        {
            // Moving left: we hit 0 when v - i ≡ 0 (mod N) for i in [1..amount]
            // Solutions: i = v + kN (v>0) or i = N + kN (v==0)
            // So:
            //   firstHit = (v == 0 ? N : v)
            //   if amount < firstHit => 0 hits
            //   else hits = 1 + floor((amount - firstHit) / N)
            int firstHit = _value == 0 ? _size : _value;
            if (amount >= firstHit)
            {
                _numZeros += 1 + (amount - firstHit) / _size;
            }
        }

        _value = ((next % _size) + _size) % _size;  // safe modulo for negatives

        if (!_countAllZeros && _value == 0)
        {
            _numZeros++;
        }

        if (_log)
        {
            Console.WriteLine("Left {0} => value {1}, zeros {2}", amount, _value, _numZeros);
        }
    }
}