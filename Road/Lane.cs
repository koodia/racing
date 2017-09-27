using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Lane : Path
{
    [Header("Lane qualities")]
    [SerializeField]
    LaneType laneType = LaneType.MotorwayLane;
    [SerializeField]
	RoadType roadType = RoadType.Tarmac;
    [SerializeField]
    int laneSpeed;
	[SerializeField]
	int laneIndex = 0; //TODO: limit traffic speed

    //public List<Transform> GetNodes()
    //{
    //    return base.nodes;
    //}

	public RoadNode[] GetRoadNodes()
	{
		return roadNodes;
	}

	public RoadNode GetFirstNode()
	{
		return roadNodes[0];
	}

    public Lane()
    {
        laneType = LaneType.MotorwayLane;
        roadType = RoadType.Tarmac;
    }

	void Awake()
	{
		CreateTransformNodes(laneIndex);
		nodes = CollectTempNodes();
	}

    void Start()
    {
		roadNodes = CreateRoadNodesFromTempNodes();
        DestroyTempNodesAndTheList();
    }
}