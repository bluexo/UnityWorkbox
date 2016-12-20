using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR

#endif

public class Node : IComparable<Node>
{
    /// <summary>
    /// 移动代价
    /// </summary>
    public int F { get { return H + G; } }

    /// <summary>
    /// 距离终点的距离
    /// </summary>
    public int H;

    /// <summary>
    /// 距离起点的距离
    /// </summary>
    public int G;

    public Vector2 Position;

    public Node Parent;

    public int CompareTo(Node other)
    {
        return F.CompareTo(other.F);
    }

    public class ReverseComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            return y.CompareTo(x);
        }
    }

    public class StepComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            return y.G.CompareTo(x.G);
        }
    }
}

[ExecuteInEditMode]
public class AStar : MonoBehaviour
{
    private Dictionary<Vector2, Color> points = new Dictionary<Vector2, Color>()
    {
        { new Vector2(0,0) , Color.cyan },
        { new Vector2(1,0) , Color.green },
        { new Vector2(2,0) , Color.red },
        { new Vector2(3,0) , Color.red },
        { new Vector2(4,0) , Color.green },

        { new Vector2(0,1) , Color.green },
        { new Vector2(1,1) , Color.green },
        { new Vector2(2,1) , Color.green },
        { new Vector2(3,1) , Color.green },
        { new Vector2(4,1) , Color.red },

        { new Vector2(0,2) , Color.red },
        { new Vector2(1,2) , Color.red },
        { new Vector2(2,2) , Color.red },
        { new Vector2(3,2) , Color.red },
        { new Vector2(4,2) , Color.red },

        { new Vector2(0,3) , Color.green },
        { new Vector2(1,3) , Color.green },
        { new Vector2(2,3) , Color.red },
        { new Vector2(3,3) , Color.green },
        { new Vector2(4,3) , Color.green },

        { new Vector2(0,4) , Color.red },
        { new Vector2(1,4) , Color.green },
        { new Vector2(2,4) , Color.green },
        { new Vector2(3,4) , Color.green },
        { new Vector2(4,4) , Color.cyan },
    };

    private List<Node> openList = new List<Node>();
    private List<Node> closeList = new List<Node>();
    private List<Vector3> path = new List<Vector3>();
    private const int MaxSearchStep = 100;
    private int currentStep = 0;
    private const int GridSize = 5;
    public Vector2 startPosition, endPosition;
    public Node beginNode;
    private bool search = true;
    private readonly Vector2 offset = new Vector2(0.5f, 0.5f);

    private void Start()
    {
        beginNode = new Node() { G = 0, H = GetHeuristic(startPosition), Position = startPosition };
        openList.Add(beginNode);
        StartCoroutine(Search());
    }

    private IEnumerator Search()
    {
        while (search || openList.Count > 0) {
            var openNode = openList.First();
            SearchNearbyNode(openNode);
            openList.Sort();
            if (!closeList.Contains(openNode)){
                openList.Remove(openNode);
                closeList.Add(openNode);
                if (openNode.Position == endPosition) {
                    openList.Clear();
                    break;
                }
            }
            Debug.LogFormat("G:{0},H:{1},F:{2},POS:{3}", openNode.G, openNode.H, openNode.F, openNode.Position);
            yield return new WaitForSeconds(.2f);
        }
        path.Add(startPosition + offset);
        Node node = closeList.Last();
        do {
            path.Add(node.Position + offset);
            node = node.Parent;
        }
        while (node.Position != beginNode.Position);
    }

    private int GetHeuristic(Vector2 currentPosition)
    {
        return (int)(Mathf.Abs(endPosition.x - currentPosition.x) + Mathf.Abs(endPosition.y + currentPosition.y));
    }

    private void SearchNearbyNode(Node prevNode)
    {
        if (currentStep > MaxSearchStep) {
            Debug.Log("Cannot find endPoint!");
            search = false;
            return;
        }
        AddNode(prevNode, Vector2.up);
        AddNode(prevNode, Vector2.down);
        AddNode(prevNode, Vector2.left);
        AddNode(prevNode, Vector2.right);
        currentStep++;
    }

    private void AddNode(Node parent, Vector2 dir)
    {
        var position = parent.Position + dir;
        if (points.ContainsKey(position)) {
            var node = new Node() { G = parent.G + 1, H = GetHeuristic(position), Position = position, Parent = parent };
            if (points[position] != Color.red
                && !openList.Exists(n => n.Position == node.Position)) {
                openList.Add(node);
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var pair in points) {
            RichText.DrawCross(pair.Key + new Vector2(.5f, .5f), pair.Value);
            Gizmos.DrawWireCube(pair.Key + new Vector2(.5f, .5f), new Vector2(.5f, .5f));
        }
        foreach (var node in closeList) {
            Gizmos.DrawWireCube(node.Position + new Vector2(.5f, .5f), new Vector2(.25f, .25f));
        }

        RichText.DrawPath(path.ToArray(), Color.yellow);
    }
}