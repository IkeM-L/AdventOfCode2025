namespace AdventOfCode2025;

public class Day8
{
    private const bool Log = true;
    
    public static long GetProductLargestCircuits(string input, int numToConnect)
    {
        var lines = input
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        List<Node> nodes = new();
        foreach (var line in lines)
        {
            var parsed = line
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
            
            nodes.Add(new Node(parsed[0], parsed[1], parsed[2]));
        }

        var junctionNodes = new JunctionNodes(nodes);

        if (Log)
        {
            var closest = junctionNodes.GetNClosest(numToConnect);
            foreach (var nodePair in closest)
            {
                Console.WriteLine($"dist: {nodePair.dist}, {nodePair.a}:{nodePair.b}");
            }
        }

        // Build connected components from the closest pairs
        var collections = junctionNodes.BuildComponentsFromNClosestPairs(numToConnect);

        var largest = collections
            .OrderByDescending(x => x.Count)
            .Take(3)
            .ToList();

        if (Log)
        {
            foreach (var set in largest)
            {
                Console.WriteLine(set.Count);
            }
        }

        return largest.Aggregate<HashSet<Node>?, long>(1, (current, set) => current * set.Count);
    }
}

public class Node : IEquatable<Node>
{
    private readonly int _x;
    private readonly int _y;
    private readonly int _z;

    public Node(int x, int y, int z)
    {
        _x = x;
        _y = y;
        _z = z;
    }

    public float DistanceTo(Node other)
    {
        float dx = _x - other._x;
        float dy = _y - other._y;
        float dz = _z - other._z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    public override string ToString()
    {
        return $"[{_x},{_y},{_z}]";
    }

    public bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _x == other._x && _y == other._y && _z == other._z;
    }

    public override bool Equals(object? obj) => Equals(obj as Node);

    public override int GetHashCode() => HashCode.Combine(_x, _y, _z);
}

public class JunctionNodes
{
    private readonly List<Node> _nodes;
    private List<(float dist, Node a, Node b)>? _pairs;
    // Undirected adjacency list
    private readonly Dictionary<Node, HashSet<Node>> _adj = new();
    private int _adjInitilisedUpTo;

    public JunctionNodes(IEnumerable<Node> nodes)
    {
        _nodes = nodes.ToList();
    }

    /// <summary>
    /// Ensure _pairs is populated and sorted by distance ascending.
    /// </summary>
    private void EnsurePairs()
    {
        if (_pairs != null)
            return;

        _pairs = new List<(float dist, Node a, Node b)>();

        // Generate all unordered pairs (i < j), compute distances
        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                var a = _nodes[i];
                var b = _nodes[j];
                float dist = a.DistanceTo(b);
                _pairs.Add((dist, a, b));
            }
        }

        _pairs.Sort((p1, p2) => p1.dist.CompareTo(p2.dist));
    }

    public List<(float dist, Node a, Node b)> GetNClosest(int n)
    {
        EnsurePairs();

        if (n < 0) n = 0;
        if (n > _pairs!.Count) n = _pairs.Count;

        return _pairs
            .Take(n)
            .ToList();
    }

    public List<HashSet<Node>> BuildComponentsFromNClosestPairs(int n)
    {
        EnsurePairs();

        if (n < 0) n = 0;
        if (n > _pairs!.Count) n = _pairs.Count;

        // Rebuild adjacency from scratch using first n pairs
        _adj.Clear();

        for (int i = 0; i < n; i++)
        {
            var (_, a, b) = _pairs[i];
            AddEdge(a, b);
            AddEdge(b, a);
        }

        _adjInitilisedUpTo = n;

        return ComputeComponents();
    }

    public List<HashSet<Node>> AddNextNodeToComponents()
    {
        EnsurePairs();

        // No more pairs to add: just return the components of the current graph
        if (_adjInitilisedUpTo >= _pairs!.Count)
        {
            return ComputeComponents();
        }

        // Add exactly the next closest pair
        var (_, a, b) = _pairs[_adjInitilisedUpTo];
        AddEdge(a, b);
        AddEdge(b, a);
        _adjInitilisedUpTo++;

        return ComputeComponents();
    }

    private void AddEdge(Node u, Node v)
    {
        if (!_adj.TryGetValue(u, out var set))
        {
            set = new HashSet<Node>();
            _adj[u] = set;
        }

        set.Add(v);
    }

    private List<HashSet<Node>> ComputeComponents()
    {
        var components = new List<HashSet<Node>>();
        var visited = new HashSet<Node>();

        foreach (var start in _adj.Keys)
        {
            if (!visited.Add(start))
                continue;

            var comp = new HashSet<Node>();
            var stack = new Stack<Node>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!comp.Add(node))
                    continue;

                foreach (var neigh in _adj[node])
                {
                    if (visited.Add(neigh))
                        stack.Push(neigh);
                }
            }

            components.Add(comp);
        }

        return components;
    }
}

