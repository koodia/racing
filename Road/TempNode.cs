using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TempNode : MonoBehaviour {

	[SerializeField]
	public RoadNode roadNode;

	// Use this for initialization
	void Start() 
	{
		roadNode.pos = gameObject.transform.position;
	}
	
	//public void Destroy()
	//{
	//	Destroy(gameObject);
	//	print("Node destoyed for good!");
	//}
}
