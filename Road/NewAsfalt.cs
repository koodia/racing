using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// First car's collider will activate the boost, but any can exit.TODO: Fix later
/// </summary>
public class NewAsfalt : MonoBehaviour //create some baseclass, basecar should use this as well
{
    private PlayerController racer;
    private PlayerController boostingPlayer;
    bool boosting;

    private void OnTriggerEnter(Collider col)
    {
        racer = col.transform.root.GetChild(1).GetComponent<PlayerController>(); //GetComponent<IRacer>();
        if (racer != null)
        {
            if (!boosting)
            {
                BoostSpeed(racer);
                boosting = true;
            }
        }
    }

    private void BoostSpeed(PlayerController racer)
    {
        this.boostingPlayer = racer;
        Debug.Log("ActivateBoost called:");
        racer.movement.Drivetrain.ActivateBoost(10f);
    }

    private void OnTriggerExit(Collider col)
    {
        if (boosting)
        { 
            Debug.Log("ActivateBoost normalized");
            boostingPlayer.movement.Drivetrain.ActivateBoost(1);
            boosting = false;
        }
    }
}

