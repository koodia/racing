using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path : MonoBehaviour {

    [Header("Path creation")]
    [SerializeField]
    public GameObject nodePrefab = null;
    [SerializeField]
    protected List<Transform> nodes = new List<Transform>();
    [SerializeField]
    protected RoadNode[] roadNodes;
    [SerializeField]
    public int numberOfNodesToGenerate = 40; //Take from level length or generate
    private float distance = 300;
    [SerializeField]
    public float distanceBetweenNodes = 500;
    private const int nodeHeight = 5; //Set about middle of the car
    public bool roadDirectionOpposite; // bool should do

    protected void CreateTransformNodes(int laneIndex)
    {
        int count = 0;
        List<Transform> manuallyAddedNodes = CollectTempNodes();
        if (manuallyAddedNodes != null && manuallyAddedNodes.Count != 0)
        {
            count = manuallyAddedNodes.Count;
            distance = manuallyAddedNodes[count - 1].position.z + 200; //Move the starting position a bit further (currently going negative, FIX PLZ)
        }

        for (int i = count; i < numberOfNodesToGenerate; i++) //Start creating nodes after the last manually set node
        {
            GameObject go = Instantiate(nodePrefab, new Vector3(transform.position.x, 2, distance), Quaternion.identity, gameObject.transform); //Quaternion.identity);
            go.GetComponent<TempNode>().roadNode.nodeNumber = i; //save the node number
            go.GetComponent<TempNode>().roadNode.laneIndex = laneIndex;
            go.GetComponent<TempNode>().roadNode.speed = 0; //Carful with this one =)
            go.gameObject.name = go.gameObject.name.Replace("(Clone)", " (" + i.ToString() + ") Distance:" + distance + " ~feet");
            distance += distanceBetweenNodes;
        }
    }

    protected List<Transform> CollectTempNodes()
    {
        //Path class creates the nodes and here we collect them to nodes list
        Transform[] pathTranforms = GetComponentsInChildren<Transform>();
        List<Transform> tempNodes = new List<Transform>();

        for (int i = 0; i < pathTranforms.Length; i++)
        {
            if (pathTranforms[i] != transform)//not adding the parent
            {
                if (pathTranforms[i].name != "Cube") //and not the child cube
                    tempNodes.Add(pathTranforms[i]);
            }
        }

        return tempNodes;
    }

    /// <summary>
    /// When we are not developing we dont need the transform and monobehaviour, so get rif of them to save performance
    /// </summary>
    /// <returns></returns>
    protected RoadNode[] CreateRoadNodesFromTempNodes()
    {
        RoadNode[] roadNodes = new RoadNode[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            roadNodes[i] = new RoadNode();
            roadNodes[i] = RoadNode.DeepCopy(nodes[i]);
        }

        return roadNodes;
    }

    public void DestroyTempNodesAndTheList()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            Destroy(nodes[i].gameObject);
        }

        nodes.Clear();  //This list is history!
        nodes = null; 
        System.GC.Collect(); //This is probably does more harm
    }


    private void OnDrawGizmosSelected()
    {
        //DrawFromTranforms()
        DrawGizmoFromNodeToNode();
    }

    [System.Obsolete("Method1 is deprecated. Gizmo draw is not drawn from Tranform anymore. Please use DrawGizmoFromNodeToNode() instead.")]
    //public void DrawFromTranforms()
    //{
    //    Gizmos.color = lineColor;
    //    Transform[] pathTranforms = GetComponentsInChildren<Transform>();
    //    nodes = new List<Transform>();

    //    for (int i = 0; i < pathTranforms.Length; i++)
    //    {
    //        if (pathTranforms[i] != transform)//not adding the parent
    //        {
    //            nodes.Add(pathTranforms[i]);
    //        }
    //    }

    //    DrawGizmoDrawSingleLine();
    //}

    /// <summary>
    /// Debugging purposes
    /// </summary>
    private void DrawGizmoSingleLine()
    {
        //Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position + new Vector3(0, nodeHeight, 0), new Vector3(transform.position.x, nodeHeight, 8000));
    }

    /// <summary>
    /// Debugging purposes
    /// </summary>
    //private void DrawGizmoFromNodeToNode()
    //{
    //    for (int i = 0; i < nodes.Count; i++)
    //    {
    //        Vector3 currentNode = nodes[i].position;
    //        Vector3 previousNode = Vector3.zero;
    //        if (i > 0 && i <= nodes.Count - 1)
    //        {
    //            Gizmos.color = Color.yellow;
    //            previousNode = nodes[i - 1].position;
    //            Gizmos.DrawLine(previousNode, currentNode);
    //        }
    //        Gizmos.DrawWireSphere(currentNode, 2.0f);
    //    }
    //}

    /// <summary>
    /// Debugging purposes
    /// </summary>
    private void DrawGizmoFromNodeToNode()
    {
        for (int i = 0; i < roadNodes.Length; i++)
        {
            Vector3 currentNode = roadNodes[i].pos;
            Vector3 previousNode = Vector3.zero;
            if (i > 0 && i <= roadNodes.Length - 1)
            {
                Gizmos.color = Color.yellow;
                previousNode = roadNodes[i - 1].pos;
                Gizmos.DrawLine(previousNode, currentNode);
            }
            Gizmos.DrawWireSphere(currentNode, 2.0f);
        }
    }

  

}
