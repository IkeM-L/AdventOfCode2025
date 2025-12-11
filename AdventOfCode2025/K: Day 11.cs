namespace AdventOfCode2025;

public class Day11
{
    private const bool Log = false;

    public static long GetNumPaths(string input, bool isPart1)
    {
        
        var (adjMatrix, indexByVertex) = ToAdjMatrix(input);
        
        
        if (isPart1)
        {
            return NumPaths("you", "out", adjMatrix, indexByVertex);
        }
        else
        {
            var svrToDac = NumPaths("svr", "dac", adjMatrix, indexByVertex);
            if (Log) Console.WriteLine($"svr -> dac: {svrToDac}");

            var svrToFft = NumPaths("svr", "fft", adjMatrix, indexByVertex);
            if (Log) Console.WriteLine($"svr -> fft: {svrToFft}");

            var fftToDac = NumPaths("fft", "dac", adjMatrix, indexByVertex);
            if (Log) Console.WriteLine($"fft -> dac: {fftToDac}");

            var dacToFft = NumPaths("dac", "fft", adjMatrix, indexByVertex);
            if (Log) Console.WriteLine($"dac -> fft: {dacToFft}");

            var fftToOut = NumPaths("fft", "out", adjMatrix, indexByVertex);
            if (Log) Console.WriteLine($"fft -> out: {fftToOut}");

            var dacToOut = NumPaths("dac", "out", adjMatrix, indexByVertex);
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

    private static long NumPaths(string start, string end, int[][] adjMatrix, Dictionary<string, int> indexByVertex)
    {
        

        if (!indexByVertex.TryGetValue(start, out var startIndex) ||
            !indexByVertex.TryGetValue(end, out var endIndex))
            return 0; // or throw, depending on what you want

        var n = adjMatrix.Length;

        // Convert int[][] -> long[,]
        var A = new long[n, n];
        for (var i = 0; i < n; i++)
        {
            var row = adjMatrix[i];
            for (var j = 0; j < n; j++)
                A[i, j] = row[j];
        }

        // current = A^1 initially
        var current = (long[,])A.Clone();

        var total = current[startIndex, endIndex]; // paths of length 1

        // In a DAG, max path length <= n-1
        for (var len = 2; len <= n - 1; len++)
        {
            current = MatrixUtils.Multiply(current, A); // now current = A^len
            total += current[startIndex, endIndex];
        }

        return total;
    }

    private static (int[][] adjMatrix, Dictionary<string, int> indexByVertex) ToAdjMatrix(string input)
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
        var indexByVertex = new Dictionary<string, int>(vertexList.Length);
        for (var i = 0; i < vertexList.Length; i++)
            indexByVertex[vertexList[i]] = i;

        var n = vertexList.Length;
        var adjMatrix = new int[n][];
        for (var i = 0; i < n; i++)
            adjMatrix[i] = new int[n];

        // Fill matrix
        foreach (var line in lines)
        {
            var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;

            var from = parts[0];
            if (!indexByVertex.TryGetValue(from, out var row))
                continue;

            var targets = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in targets)
            {
                if (!indexByVertex.TryGetValue(t, out var col))
                    continue;

                adjMatrix[row][col] = 1;
            }
        }

        return (adjMatrix, indexByVertex);
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