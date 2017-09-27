
using System.Collections;
using UnityEngine;

/// <summary>
/// A super class for each road object. Currently for cars only, so TODO!
/// </summary>
//[RequireComponent(typeof(Rigidbody))]
public class RoadObject : MonoBehaviour
{
    public Rigidbody rigid;
    private Rigidbody targetCollisionObject;
    private Rigidbody previousTargetCollisionObject;
    private BaseCar baseCar;
    private bool isThePlayer = false;
    private bool sameObjectLock = false;
    private float collisionMagnitude = 0;

    void Awake()
    {
        rigid = transform.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Needed when a prefab traffic is turned into player racecar
    /// </summary>
    public void RefreshRoadObject()
    {
        rigid = transform.GetComponent<Rigidbody>();

        if (rigid == null) //Its the formula
        {
            rigid = transform.GetChild(0).transform.Find("Body").GetComponent<Rigidbody>();
        }
        Debug.Assert(rigid != null, "rigidbody is null");

        baseCar = transform.GetComponent<PlayerController>();
        Debug.Assert(baseCar != null, "player is null." + transform.name);
        isThePlayer = true;
    }

    private bool AllowToHitWithTheSameTargetInARow()
    {
        if (previousTargetCollisionObject != null)
        {
            if (previousTargetCollisionObject.GetInstanceID() == targetCollisionObject.GetInstanceID() && sameObjectLock)
            {
                return false;
            }
        }

        return true;
    }

    IEnumerator TakeTime(float lockTime)
    {
        yield return new WaitForSeconds(lockTime);
        previousTargetCollisionObject = null;
        sameObjectLock = false;
    }

    /// <summary>
    /// A monster performance eater. TODO: Optimize this like no-other. Currently some useless if's
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollisionEnter(Collision collision)
    {
        if (isThePlayer)
        {
            if(!collision.transform.CompareTag("Terrain") && collision.gameObject.layer != LayerMask.NameToLayer("NoDamage"))
            {
                Debug.Assert(baseCar != null, "baseCar is null");

                    targetCollisionObject = collision.transform.GetComponent<Rigidbody>();
                    if (targetCollisionObject != null)
                    {
                        if (AllowToHitWithTheSameTargetInARow() == true)
                        {
                        collisionMagnitude = collision.relativeVelocity.magnitude * targetCollisionObject.mass; //baseCar.rigid.mass;
                        if ((collisionMagnitude) > Armor.hitThreshold)
                            {
                               // Debug.Log(collision.relativeVelocity.magnitude * baseCar.rigdig.mass);
                                previousTargetCollisionObject = targetCollisionObject;
                                baseCar.Armor.TakeHitOrDamage(isThePlayer, collisionMagnitude);
                                sameObjectLock = true;
                                StartCoroutine(TakeTime(1.5f)); //seconds until you can hit the same object in a row
                            }
                        }
                    }
            }
        }
    }




}
