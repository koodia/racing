using System;
using System.Collections;
using UnityEngine;

/// <summary>
///  There are only 4 parts to be rendered all times. One in which player is currently a, the next that is comming up and one behind
/// </summary>
public class RoadManager : MonoBehaviour //Remove the Monobeviour, the class content dont needed it!
{
    public Transform[] roadParts;
    private int[] activeParts = new int[4];
   // public int[] oldParts = new int[3];
    [SerializeField]
    //public GameObject playerObject; //currently using Main camera
    public string playerCurrentlyAtPart = "";
    float timer = 0; //temp rubbish;
    public static RoadManager instance;

    //void Awake()
    //{
    //    //Add the parts manually until you find a way
    //    //roadParts = road.GetComponents<Transform>();
    //    //roadParts = road.transform.GetComponents<Transform>();
    //    //foreach (object road in road.transform)
    //    //{
    //    //    Transform child = (Transform)road;
    //    //    roadParts[0] = child;
    //    //    break;
    //    //}
    //}

    void Start()
    {
        instance = this;

        //If you start at the beginng of the road the parts 1,2 and 3 should be active
        //UpdateRoadManager(Camera.main.transform.position); //Throws some null error, probably the Camera has not been set yet
    }



    /// <summary>
    /// TODO: This update is a temp thing to keep the road updated
    /// </summary>
    private void FixedUpdate()
    {
        if (!GameManager.dontEmulateTraffic)
        {
            timer += Time.deltaTime;
            if (timer >= 3f)
            {
                UpdateRoadManager(Camera.main.transform.position);
                timer = 0.0f;
            }
        }
    }


    public Transform GetClosestRoadPart(float carPosZ)
    {
        for (int i = 0; i < roadParts.Length; i++)
        {
            if (roadParts[i].transform.position.z > carPosZ)
            {
                return roadParts[i].transform;
            }
        }
      
        throw new Exception("oooooooooooo what?");
    }

    /// <summary>
    /// Triggers everything. Call this whenever you feel like it
    /// </summary>
    /// <param name="playerPos"></param>
    public void UpdateRoadManager(Vector3 playerPos) 
    {
        CheckPartStatus(playerPos);
        ActivateParts();
        DisablePartsThatAreNotInActiveList(); //Parts are out of field of view
    }

    private void CheckPartStatus(Vector3 playerPos)
    {
        int nearestPart = ClosestTo(roadParts, playerPos.z);
        playerCurrentlyAtPart = roadParts[nearestPart].gameObject.name;

        //Set in activation order. If we activate the child objects on by one in the future
        activeParts[0] = (nearestPart + 2 > roadParts.Length) ? roadParts.Length : nearestPart + 2; //In the far future
        activeParts[1] = (nearestPart + 1 > roadParts.Length) ? roadParts.Length : nearestPart + 1; //The next one, this must be activated first
        activeParts[2] = (nearestPart - 1 < 0) ? 0 : nearestPart - 1; //road part behind
        activeParts[3] = nearestPart; //nearest, but already rendering
    }

    /// <summary>
    /// This could be moved to Helpers.cs
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int ClosestTo(Transform[] collection, float target)
    {
        // NB Method will return int.MaxValue for a sequence containing no elements.
        int closestIndex = -1;
        var minDifference = int.MaxValue;
   
        for (int i = 0; i < collection.Length; i++)
        {
            var difference = Math.Abs((long)collection[i].position.z - target);
            if (minDifference > difference)
            {
                minDifference = (int)difference;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="number"></param>
    /// <param name="lookUpArray"></param>
    /// <returns></returns>
    private bool IsValueInArray(int number, int[] lookUpArray)
    {
       
        for (int i = 0; i < lookUpArray.Length; i++)
        {
            if (lookUpArray[i] == number)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    private void DisablePartsThatAreNotInActiveList()
    {
        for (int i = 0; i < roadParts.Length; i++)
        {
            if (IsValueInArray(i, activeParts) == false) // If partNumber is not found from current activePart list, then disable it
            {
                StartCoroutine(ActivatePart(i, false));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ActivateParts()
    {
        for (int i = 0; i < activeParts.Length; i++)
        {
            if (roadParts[activeParts[i]].gameObject.activeSelf == false)  //Dont start rendering if the part is already rendering
            {
                StartCoroutine(ActivatePart(activeParts[i], true));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="partNumberIndex"></param>
    /// <param name="activate"></param>
    private void PartActivation(int partNumberIndex, bool activate)
    {
        if (activate)
        {
              if (roadParts[partNumberIndex].gameObject.activeSelf == false)  //Dont start rendering if the part is already rendering
              {
                StartCoroutine(ActivatePart(partNumberIndex, true));
              }
        }
    }

    /// <summary>
    /// There could hundreds of objects to activate in the future?
    /// </summary>
    /// <param name="index"></param>
    /// <param name="active"></param>
    /// <returns></returns>
    private IEnumerator ActivatePart(int index, bool active)
    {
        if (active)
        {
            roadParts[index].gameObject.SetActive(active);
        }
        else
        {
            roadParts[index].gameObject.SetActive(active);
        }
        yield return null;
    }
}
