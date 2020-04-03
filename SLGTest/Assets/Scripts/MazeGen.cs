using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum NodeType
{
    NOT_VISITED,
    VISITED
}


public struct WallConfig
{
    public bool top;
    public bool left;
    public bool bot;
    public bool right;
}

public class Node
{
    public int x;
    public int y;
    public NodeType nodeType;
    public Transform nodeTransform;
    public WallConfig wallConfig;
    public List<Node> neighbours = new List<Node> ();

    public Node (int x, int y, GameObject g)
    {
        this.x = x;
        this.y = y;
        this.nodeType = NodeType.NOT_VISITED;
        this.nodeTransform = g.transform;
        this.wallConfig = new WallConfig ();
        this.wallConfig.top = true;
        this.wallConfig.left = true;
        this.wallConfig.bot = true;
        this.wallConfig.right = true;
    }

}


public class MazeGen : MonoBehaviour
{
    public int GridSize; 

    public GameObject PrefabNode;

    private Node[,] nodeList;

    private Stack<Node> stack = new Stack<Node> ();

    private void Start()
    {
        nodeList = new Node[GridSize, GridSize];

        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                Node node = new Node (i, j, Instantiate (PrefabNode));
                node.nodeTransform.position = new Vector3 (i, 0, j);
                nodeList[i, j] = node;
            }
        }
        
        Node initialCell = nodeList[0, 0];
        
        initialCell.wallConfig.bot = false; //entrance
        initialCell.nodeType = NodeType.VISITED;
        stack.Push (initialCell);

        while (stack.Count > 0)
        {
            Node currentNode = stack.Pop ();
            Node neighbour = GetRandomNeighbor (currentNode);
            if (neighbour != null)
            {
                stack.Push (currentNode);
                stack.Push (neighbour);
            }
        }
        
        nodeList[GridSize - 1, GridSize - 1].wallConfig.top = false; // exit
        
        UpdateVisual ();
    }

    public void UpdateVisual()
    {
        //update visual
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                //OPTIMIZATION POINT, this is bad
                nodeList[i, j].nodeTransform.GetComponent<NodeVisual> ().UpdateVisual (nodeList[i, j]);
            }
        }
    }

    public Node[,] GetNodes()
    {
        return nodeList;
    }


    public Node GetRandomNeighbor (Node traverseNode)
    {
        List<Node> nList = new List<Node> ();
        //top
        if (traverseNode.y < GridSize - 1 &&
            nodeList[traverseNode.x, traverseNode.y + 1].nodeType == NodeType.NOT_VISITED)
        {
            nList.Add (nodeList[traverseNode.x, traverseNode.y + 1]);
        }

        //left
        if (traverseNode.x > 1 && nodeList[traverseNode.x - 1, traverseNode.y].nodeType == NodeType.NOT_VISITED)
        {
            nList.Add (nodeList[traverseNode.x - 1, traverseNode.y]);
        }

        //bot
        if (traverseNode.y > 1 && nodeList[traverseNode.x, traverseNode.y - 1].nodeType == NodeType.NOT_VISITED)
        {
            nList.Add (nodeList[traverseNode.x, traverseNode.y - 1]);
        }

        //right
        if (traverseNode.x < GridSize - 1 &&
            nodeList[traverseNode.x + 1, traverseNode.y].nodeType == NodeType.NOT_VISITED)
        {
            nList.Add (nodeList[traverseNode.x + 1, traverseNode.y]);
        }

        if (nList.Count > 0)
        {
            Node chosen = nList[Random.Range (0, nList.Count)];
            int diffX = chosen.x - traverseNode.x;
            int diffY = chosen.y - traverseNode.y;

            if (diffY == 1) // top
            {
                chosen.wallConfig.bot = false;
                traverseNode.wallConfig.top = false;
            }
            else if (diffX == -1) //left
            {
                chosen.wallConfig.right = false;
                traverseNode.wallConfig.left = false;
            }
            else if (diffY == -1) //bot
            {
                chosen.wallConfig.top = false;
                traverseNode.wallConfig.bot = false;
            }
            else if (diffX == 1) //right
            {
                chosen.wallConfig.left = false;
                traverseNode.wallConfig.right = false;
            }
            
            chosen.nodeType = NodeType.VISITED;
            return chosen;
        }

        return null;
    }
}