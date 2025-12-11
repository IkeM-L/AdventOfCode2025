namespace AdventOfCode2025;

public class Day11
{
    private static Dictionary<string, int>? _indexByVertex = null;
    private static int[][]? _adjMatrix = null;
    private const bool Log = true;

    public static long GetNumPaths(string input, bool isPart1)
    {
        if (isPart1)
        {
            return NumPaths(input, "you", "out");
        }
        else
        {
            var svrToDac = NumPaths(input, "svr", "dac");
            if (Log) Console.WriteLine($"svr -> dac: {svrToDac}");

            var svrToFft = NumPaths(input, "svr", "fft");
            if (Log) Console.WriteLine($"svr -> fft: {svrToFft}");

            var fftToDac = NumPaths(input, "fft", "dac");
            if (Log) Console.WriteLine($"fft -> dac: {fftToDac}");

            var dacToFft = NumPaths(input, "dac", "fft");
            if (Log) Console.WriteLine($"dac -> fft: {dacToFft}");

            var fftToOut = NumPaths(input, "fft", "out");
            if (Log) Console.WriteLine($"fft -> out: {fftToOut}");

            var dacToOut = NumPaths(input, "dac", "out");
            if (Log) Console.WriteLine($"dac -> out: {dacToOut}");

            var term1 = svrToDac * dacToFft * fftToOut; // svr -> dac -> fft -> out
            if (Log) Console.WriteLine($"term1 (svr->dac->fft->out): {term1}");

            var term2 = svrToFft * fftToDac * dacToOut; // svr -> fft -> dac -> out
            if (Log) Console.WriteLine($"term2 (svr->fft->dac->out): {term2}");

            var total = term1 + term2;
            if (Log) Console.WriteLine($"total paths svr->out via dac & fft: {total}");

            return total;
        }

        
    }

    private static long NumPaths(string input, string start, string end)
    {
        if(_adjMatrix is null || _indexByVertex is null)
        {
            // Get adjacency matrix and name->index map
            var (adjMatrix, indexByVertex) = To_adjMatrix(input);
            _adjMatrix = adjMatrix;
            _indexByVertex = indexByVertex;
        }

        if (!_indexByVertex.TryGetValue(start, out var youIndex) ||
            !_indexByVertex.TryGetValue(end, out var outIndex))
            return 0; // or throw, depending on what you want

        var n = _adjMatrix.Length;

        // Convert int[][] -> long[,]
        var A = new long[n, n];
        for (var i = 0; i < n; i++)
        {
            var row = _adjMatrix[i];
            for (var j = 0; j < n; j++)
                A[i, j] = row[j];
        }

        // current = A^1 initially
        var current = (long[,])A.Clone();

        var total = current[youIndex, outIndex]; // paths of length 1

        // In a DAG, max path length <= n-1
        for (var len = 2; len <= n - 1; len++)
        {
            current = MatrixUtils.Multiply(current, A); // now current = A^len
            total += current[youIndex, outIndex];
        }

        return total;
    }

    private static (int[][] _adjMatrix, Dictionary<string, int> _indexByVertex) To_adjMatrix(string input)
    {
        // Split into non-empty lines
        var lines = input.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        // Collect all vertex names (left and right of ':')
        var vertices = new HashSet<string>();

        foreach (var line in lines)
        {
            var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 0) continue;

            var from = parts[0];
            vertices.Add(from);

            if (parts.Length == 2)
            {
                var targets = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in targets)
                    vertices.Add(t);
            }
        }

        // Deterministic ordering of vertices
        var vertexList = vertices.OrderBy(v => v).ToArray();

        // Map vertex name -> index
        var _indexByVertex = new Dictionary<string, int>(vertexList.Length);
        for (var i = 0; i < vertexList.Length; i++)
            _indexByVertex[vertexList[i]] = i;

        var n = vertexList.Length;
        var _adjMatrix = new int[n][];
        for (var i = 0; i < n; i++)
            _adjMatrix[i] = new int[n];

        // Fill matrix
        foreach (var line in lines)
        {
            var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;

            var from = parts[0];
            if (!_indexByVertex.TryGetValue(from, out var row))
                continue;

            var targets = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in targets)
            {
                if (!_indexByVertex.TryGetValue(t, out var col))
                    continue;

                _adjMatrix[row][col] = 1;
            }
        }

        return (_adjMatrix, _indexByVertex);
    }
}

public static class MatrixUtils
{
    // Multiply two n×n matrices
    public static long[,] Multiply(long[,] A, long[,] B)
    {
        var n = A.GetLength(0);
        var C = new long[n, n];

        for (var i = 0; i < n; i++)
        for (var k = 0; k < n; k++)
        {
            var aik = A[i, k];
            if (aik == 0)
                continue;

            for (var j = 0; j < n; j++) C[i, j] += aik * B[k, j];
        }

        return C;
    }

    // Return identity matrix of size n
    private static long[,] Identity(int n)
    {
        var I = new long[n, n];
        for (var i = 0; i < n; i++)
            I[i, i] = 1;
        return I;
    }


    // A^k via exponentiation by squaring, k >= 0
    public static long[,] Power(long[,] A, long k)
    {
        var n = A.GetLength(0);
        var result = Identity(n);
        var baseM = A;

        while (k > 0)
        {
            if ((k & 1L) != 0)
                result = Multiply(result, baseM);

            baseM = Multiply(baseM, baseM);
            k >>= 1;
        }

        return result;
    }
}