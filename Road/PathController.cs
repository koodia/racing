using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathController : MonoBehaviour {

    [HideInInspector] public List<GameObject> lanes;
    public int currentLaneIndex = 0;
    //public bool playable; 

    void Awake()
    {
        UpdateLanes();
    }

    void Start()
    {
		Debug.Assert(currentLaneIndex < lanes.Count, "AI driver has invalid lane number");
    }

    private void UpdateLanes()
    {
        lanes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Lane")).OrderBy(go => go.name).ToList();

    }

    public Vector3 GetCurrentNodePosition(int currentNode)
    {
        return lanes[currentLaneIndex].GetComponent<Lane>().GetRoadNodes()[currentNode].pos;
    }

    public bool IsItPossibleToChangeRight()
    {
        //Special chewing gun
        if (currentLaneIndex == 6 && gameObject.transform.position.z < 1500) //Dont allow to switch to lane 7 at the start, or its armageddon
        {
            return false;
        }

        if (currentLaneIndex < lanes.Count - 1)
        {
            return true;
        }
        return false;
    }

    public bool IsItPossibleToChangeLeft()
    {
        if (0 < currentLaneIndex)
        {
            return true;
        }
        return false;
    }

    public Lane GetCurrentLane()
    {
        return lanes[currentLaneIndex].GetComponent<Lane>();
    }

    public int GetClosestObject(List<Transform> pathnodes, Transform fromThis)
    {
        int bestTarget = 0;
        float closestDistanceSqr = Mathf.Infinity; //TODO: Get rid off this infinity!!
        //float closestDistanceSqr = 100;
        Vector3 currentPosition = fromThis.position;
        for (int i = 0; i < pathnodes.Count; i++)
        {
            Vector3 directionToTarget = pathnodes[i].position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr )
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = i;
            }
        }
        return bestTarget;
    }

    public int GetClosestObjectNonSpecialRoadNode(RoadNode[] pathnodes, Transform go)
    {
        int bestTarget = 0;
        float closestDistanceSqr = Mathf.Infinity; //TODO: Get rid off this infinity!!
        //float closestDistanceSqr = 100;
        Vector3 currentPosition = go.position;
        for (int i = 0; i < pathnodes.Length - 1; i++)
        {
            Vector3 directionToTarget = pathnodes[i].pos - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr && pathnodes[i].sectionType == SectionType.Straight)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = i;
            }
        }
        return bestTarget;
    }



}
