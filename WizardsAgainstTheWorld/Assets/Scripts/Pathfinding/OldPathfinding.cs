using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public interface IPathfinding
{
    // List<Node> FindPath(Vector3 startPos, Vector3 targetPos);

    bool IsClearPath(Vector2 a, Vector2 b);
    bool IsWalkable(Vector2 targetPosition);

    ICollection<Vector2> GetSpreadPosition(Vector2 position, int amount);
    void AddObstacle(IObstacle obstacle);
    void RemoveObstacle(IObstacle obstacle);
}

public interface IObstacle
{
    Vector2 Position { get; }
    float Radius { get; }
    event Action Moved;
}



[RequireComponent(typeof(GridGenerator))]
public class OldPathfinding : MonoBehaviour, IPathfinding
{
    private GridGenerator _grid;

    private List<IObstacle> _obstacles = new List<IObstacle>();
    [SerializeField] private bool displayGridGizmos;

    
    public const int PathLimit = 100;
    public const int FindClosestNodeLimit = 25;
    
    void Awake()
    {
        _grid = GetComponent<GridGenerator>();
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = _grid.NodeFromWorldPoint(startPos);
        startNode = GetClosestWalkableNode(startNode); // We need to find the closest walkable node to the start
        
        Node targetNode = _grid.NodeFromWorldPoint(targetPos);
        targetNode = GetClosestWalkableNode(targetNode); // We need to find the closest walkable node to the target

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        
        int iterations = 0; // Track the number of iterations, to apply limit
        
        while (openSet.Count > 0)
        {
            // Check if the pathfinding exceeds the iteration limit
            if (iterations >= PathLimit)
            {
                GameLogger.LogWarning("Pathfinding reached the iteration limit and was stopped.");
                return new List<Node>(); // Return an empty list if the limit is exceeded
            }
            iterations++;
            
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbour in _grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour) || IsBlockedByObstacle(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        // Path not found
        return new List<Node>();
    }

    private Node GetClosestWalkableNode(Node startNode)
    {
        int checks = 0;

        foreach (var node in _grid.GetAllReachableNodesBFS(startNode))
        {
            if (node.walkable && !IsBlockedByObstacle(node))
            {
                return node;
            }

            checks++;
            if (checks > FindClosestNodeLimit)
            {
                break;
            }
        }

        return null;
    }

    private bool IsBlockedByObstacle(Node neighbour)
    {
        foreach (var obstacle in _obstacles)
        {
            if (Vector2.Distance(neighbour.worldPosition, obstacle.Position) < obstacle.Radius)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsClearPath(Vector2 a, Vector2 b)
    {
        // Calculate the direction from point a to point b
        Vector2 direction = b - a;
        float distance = direction.magnitude;

        // Perform the Raycast
        RaycastHit2D hit = Physics2D.Raycast(a, direction, distance, CollisionUtility.UnwalkableLayerMask);
        if (hit.collider != null)
        {
            // If a collider is hit on the specified layer, return false
            return false;
        }

        // If no collider is hit on the specified layer, return true
        return true;
    }

    public bool IsWalkable(Vector2 targetPosition)
    {
        Node node = _grid.NodeFromWorldPoint(targetPosition);
        return node.walkable;
    }

    public ICollection<Vector2> GetSpreadPosition(Vector2 position, int count)
    {
        var nodes = _grid.GetNodes();

        var selectedNodes = new List<Node>();
        var visitedNodes = new HashSet<Node>();
        int rows = nodes.GetLength(0);
        int cols = nodes.GetLength(1);

        // Helper method to check if a position is within bounds
        bool IsInBounds(int x, int y) => x >= 0 && y >= 0 && x < rows && y < cols;

        // Find the closest walkable node to the given position
        Node closestNode = null;
        float closestDistance = float.MaxValue;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Node node = nodes[x, y];

                if (node.walkable)
                {
                    float distance = Vector2.Distance(node.worldPosition, position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNode = node;
                    }
                }
            }
        }

        if (closestNode == null) return new List<Vector2>(); // If no walkable nodes found, return empty list

        var queue = new Queue<(Node, int, int)>();

        // Find the coordinates of the closest walkable node
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if (nodes[x, y] == closestNode)
                {
                    queue.Enqueue((closestNode, x, y));
                    visitedNodes.Add(closestNode);
                    break;
                }
            }
        }

        // Perform BFS
        while (queue.Count > 0 && selectedNodes.Count < count)
        {
            var (currentNode, x, y) = queue.Dequeue();
            selectedNodes.Add(currentNode);

            // Add neighbors (4-directional: up, down, left, right)
            foreach (var (nx, ny) in new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) })
            {
                if (IsInBounds(nx, ny))
                {
                    Node neighbor = nodes[nx, ny];
                    if (!visitedNodes.Contains(neighbor) && neighbor.walkable)
                    {
                        queue.Enqueue((neighbor, nx, ny));
                        visitedNodes.Add(neighbor);
                    }
                }
            }
        }

        return selectedNodes.Select(x => (Vector2)x.worldPosition).ToList();
    }

    public void AddObstacle(IObstacle obstacle)
    {
        _obstacles.Add(obstacle);
    }

    public void RemoveObstacle(IObstacle obstacle)
    {
        _obstacles.Remove(obstacle);
    }


    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void OnDrawGizmos()
    {
        if (!displayGridGizmos)
            return;

        Gizmos.DrawWireCube(transform.position, new Vector3(_grid.gridWorldSize.x, _grid.gridWorldSize.y, 1));

        foreach (Node n in _grid.GetNodes())
        {
            Gizmos.color = (n.walkable && !IsBlockedByObstacle(n)) ? Color.white : Color.red;
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (_grid.nodeRadius * 2 - .1f));
        }
    }
}