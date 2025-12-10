namespace AdventOfCode2025;

public class Day8
{
    private const bool Log = false;
    
    public static long Solve(string input, int numToConnect, bool isPartOne)
    {
        var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

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

        if (isPartOne)
        {
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
            
            return largest.Aggregate(1L, (current, set) => current * set.Count);
        }
        
        var output = (collections: collections, mostRecentlyAdded: (Node?)null, mostRecentlyConnectedTo: (Node?)null);

        // We assume we don't already have one component after part 1 due to the puzzle setup
        // If we do, this will be incorrect. Easy fix (just don't re-run part 1 code and start from empty) but 
        // this gets the correct answer faster 
        // We could also have done a binary search or something, I'd need to benchmark to see what is faster and am too busy today
        while (output.collections.Count != 1)
        {
            output = junctionNodes.AddNextNodeToComponents();
            if (Log)
            {
                Console.WriteLine(
                    $"Collection count: {output.collections.Count} " +
                    $"Nodes: {output.mostRecentlyAdded}:{output.mostRecentlyConnectedTo}");
            }
        }

        if (output.mostRecentlyAdded is null || output.mostRecentlyConnectedTo is null)
            throw new InvalidOperationException("No final edge to compute product from.");

        return output.mostRecentlyAdded.X * output.mostRecentlyConnectedTo.X;
    }
}

public class Node(int x, int y, int z) : IEquatable<Node>
{
    private readonly int _x = x;
    private readonly int _y = y;
    private readonly int _z = z;
    
    public long X => _x;

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

public class JunctionNodes(IEnumerable<Node> nodes)
{
    private readonly List<Node> _nodes = nodes.ToList();
    private List<(float dist, Node a, Node b)>? _pairs;

    // Undirected adjacency list
    private readonly Dictionary<Node, HashSet<Node>> _adj = new();
    private int _adjInitilisedUpTo;

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
                var dist = a.DistanceTo(b);
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

    public (List<HashSet<Node>> collections, Node? mostRecentlyAdded, Node? mostRecentlyConnectedTo) AddNextNodeToComponents()
    {
        EnsurePairs();

        // No more pairs to add: just return the components of the current graph
        if (_adjInitilisedUpTo >= _pairs!.Count)
        {
            return (ComputeComponents(), null, null);
        }

        var (_, a, b) = _pairs[_adjInitilisedUpTo];

        // Capture membership before adding the edge so we know which node is "new"
        var aExisted = _adj.ContainsKey(a);
        var bExisted = _adj.ContainsKey(b);

        AddEdge(a, b);
        AddEdge(b, a);
        _adjInitilisedUpTo++;

        Node? mostRecentlyAdded;
        Node? mostRecentlyConnectedTo;

        switch (aExisted)
        {
            case true when !bExisted:
                mostRecentlyAdded = b;
                mostRecentlyConnectedTo = a;
                break;
            case false when bExisted:
                mostRecentlyAdded = a;
                mostRecentlyConnectedTo = b;
                break;
            default:
                // Either both existed already or both are new and form a new component.
                // In either case, just return the pair in a stable order.
                mostRecentlyAdded = a;
                mostRecentlyConnectedTo = b;
                break;
        }

        return (ComputeComponents(), mostRecentlyAdded, mostRecentlyConnectedTo);
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

        // Ensure every node is represented in the adjacency map
        foreach (var node in _nodes)
        {
            if (!_adj.ContainsKey(node))
                _adj[node] = new HashSet<Node>();
        }

        // Run DFS/BFS over all nodes, not just those with edges
        foreach (var start in _nodes)
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

                foreach (Node? neigh in _adj[node].Where(neigh => visited.Add(neigh)))
                {
                    stack.Push(neigh);
                }
            }

            components.Add(comp);
        }

        return components;
    }
}

