using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class NodeVisual : MonoBehaviour
{
    public List<Transform> walls;
    public GameObject Indicator;

    private Node localNode;

    public void UpdateVisual(Node node)
    {
        localNode = node;
        walls[0].gameObject.SetActive (node.wallConfig.top);
        walls[1].gameObject.SetActive (node.wallConfig.left);
        walls[2].gameObject.SetActive (node.wallConfig.bot);
        walls[3].gameObject.SetActive (node.wallConfig.right);
        
        //Indicator.SetActive (node.nodeType == NodeType.VISITED);
        Indicator.SetActive (false);
    }

    public Vector2Int GetGridPos()
    {
        return  new Vector2Int(localNode.x, localNode.y);
    }
    
}
