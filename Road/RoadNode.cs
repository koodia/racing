
using UnityEngine;



[System.Serializable]
public class RoadNode
{
	[SerializeField]
	public int laneIndex;
	[SerializeField]
	public int nodeNumber;
	[SerializeField]
	public int speed;
	[SerializeField]
	public bool dontAllowLaneSwitching;
	[SerializeField]
	public bool disableRacersSideSensors;
	[SerializeField]
	public SectionType sectionType;
	public Vector3 pos { get; set;}

    public static RoadNode DeepCopy(Transform node)
    {
        RoadNode dataFromTempNode = node.transform.GetComponent<TempNode>().roadNode;

        RoadNode temp = new RoadNode();
        temp.laneIndex = dataFromTempNode.laneIndex;
        temp.nodeNumber = dataFromTempNode.nodeNumber;
        temp.speed = dataFromTempNode.speed;
        temp.dontAllowLaneSwitching = dataFromTempNode.dontAllowLaneSwitching;
        temp.disableRacersSideSensors = dataFromTempNode.disableRacersSideSensors;
        temp.sectionType = dataFromTempNode.sectionType;
        temp.pos = node.position; 
        return temp;
    }

}
