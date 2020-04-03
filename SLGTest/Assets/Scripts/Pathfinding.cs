using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public MazeGen maze;

    public Vector2Int currentDestination = new Vector2Int (5, 5);

    private bool isNeighboursUpdated = false;
    private Camera _mainCam;

    private void Start()
    {
        _mainCam = Camera.main;
    }
    
    public void UpdateNeighbours()
    {
        if (isNeighboursUpdated)
        {
            return;
        }

        isNeighboursUpdated = true;
        Node[,] nodes = maze.GetNodes ();

        for (int i = 0; i < nodes.GetLength (0); i++)
        {
            for (int j = 0; j < nodes.GetLength (1); j++)
            {
                if (i > 0) //left
                {
                    if (!nodes[i, j].wallConfig.left)
                    {
                        nodes[i, j].neighbours.Add (nodes[i - 1, j]);
                    }
                }

                if (i < (nodes.GetLength (0) - 1)) // right
                {
                    if (!nodes[i, j].wallConfig.right)
                    {
                        nodes[i, j].neighbours.Add (nodes[i + 1, j]);
                    }
                }

                if (j > 0) //bot
                {
                    if (!nodes[i, j].wallConfig.bot)
                    {
                        nodes[i, j].neighbours.Add (nodes[i, j - 1]);
                    }
                }
                
                if (j < (nodes.GetLength (1) - 1)) //top
                {
                    if (!nodes[i, j].wallConfig.top)
                    {
                        nodes[i, j].neighbours.Add (nodes[i, j + 1]);
                    }
                }
            }
        }
    }


    public void FindPath()
    {
        UpdateNeighbours ();
        Node[,] nodes = maze.GetNodes ();
        
        Node startNode = nodes[0, 0];
        Node destinationNode = nodes[currentDestination.x, currentDestination.y];

        List<Node> openNodes = new List<Node> ();   //open set
        List<Node> closedNodes = new List<Node> (); //closed set
        
        Dictionary<Node, float> heuristicCosts = new Dictionary<Node, float> ();
        Dictionary<Node, float> distanceCosts = new Dictionary<Node, float> ();
        Dictionary<Node, Node> backtrack = new Dictionary<Node, Node> ();
        
        distanceCosts[startNode] = 0;
        backtrack[startNode] = null;

        //calculate heuristics => distance from end node
        for (int i = 0; i < nodes.GetLength (0); i++)
        {
            for (int j = 0; j < nodes.GetLength (1); j++)
            {
                float h = GetMovementCost (nodes[i,j], nodes[currentDestination.x, currentDestination.y]);
                heuristicCosts.Add (nodes[i, j], h);

                if (nodes[i, j] != startNode)
                {
                    distanceCosts.Add (nodes[i, j], Mathf.Infinity);
                    backtrack[nodes[i, j]] = null;
                }
            }
        }

        Debug.Log ("heuristics measurement done");
        openNodes.Add (startNode);

        while (true)
        {
            Node u = null;//traversenode
            float fCost = 0; //lowest cost of g+h

            foreach (var oN in openNodes)
            {
                if (u == null)
                {
                    u = oN;
                    fCost = heuristicCosts[u] + distanceCosts[u];
                    continue;
                }

                float fc = heuristicCosts[oN] + distanceCosts[oN];
                if (fc < fCost)
                {
                    u = oN;
                    fCost = heuristicCosts[u] + distanceCosts[u];
                }
            }


            if (u == destinationNode)
            {
                Debug.Log ("found destination");
                break;
            }

            openNodes.Remove (u);
            closedNodes.Add (u);

            foreach (var neighbour in u.neighbours)
            {
                //is this node newly found
                if (!openNodes.Contains (neighbour) && !closedNodes.Contains (neighbour))
                {
                    openNodes.Add (neighbour);
                    distanceCosts[neighbour] =
                        distanceCosts[u] + GetMovementCost (u, neighbour);
                    backtrack[neighbour] = u;
                }
            }


            //update new cost value
            foreach (var oN in openNodes)
            {
                if (!u.neighbours.Contains (oN))
                {
                    continue;
                }

                float newGCost = distanceCosts[u] + GetMovementCost (u, oN);
                if (newGCost < distanceCosts[oN])
                {
                    distanceCosts[oN] = newGCost;
                    backtrack[oN] = u;
                }
            }

            if (openNodes.Count <= 0)
            {
                break;
            }
            
        }

        Debug.Log ("should find a path here");
        List<Node> chosenPath = new List<Node> ();
        Node cNode = destinationNode;

        while (cNode != null)
        {
            chosenPath.Add (cNode);
            cNode = backtrack[cNode];
        }

        chosenPath.Reverse ();
        ShowPathVisuals (nodes, chosenPath);
    }

    public void ShowPathVisuals (Node[,] fullMaze, List<Node> chosenPath)
    {
        for (int i = 0; i < fullMaze.GetLength (0); i++)
        {
            for (int j = 0; j < fullMaze.GetLength (1); j++)
            {
                fullMaze[i, j].nodeTransform.GetComponent<NodeVisual> ().Indicator
                    .SetActive (chosenPath.Contains (fullMaze[i, j]));
            }
        }
    }


    public float GetMovementCost (Node n1, Node n2)
    {
        return Vector3.Distance (n1.nodeTransform.position, n2.nodeTransform.position);
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown (0)) //can be optimized further
        {
            //detect maze index
            Ray ray = _mainCam.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast (ray, out hit))
            {
                Debug.Log (hit.transform.name);
                NodeVisual nv = hit.transform.GetComponent<NodeVisual> ();
                if (nv != null)
                {
                    currentDestination = nv.GetGridPos ();
                    FindPath ();    
                }
            }
            
        }
    }
}