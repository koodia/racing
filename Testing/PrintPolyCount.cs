using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintPolyCount : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        var filters = gameObject.GetComponentsInChildren<MeshFilter>();
        int totalTriangles = 0;

        foreach (var item in filters)
        {
            //Debug.Log(item.name);
            totalTriangles += item.mesh.triangles.Length / 3;
        }

        Debug.Log(gameObject.name + ". Number of meshFilters:" + filters.Length + ". Triangles:" + totalTriangles);

    }
}
