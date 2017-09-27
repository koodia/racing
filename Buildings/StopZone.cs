using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopZone : MyMonoBase
{
    private float startTimer = 0;
    private float recoveryCycleWaitTime;
    bool repairing = false;
    private IRacer racer;
    public GlowMateria glowingScript;

    //basecolor= 00FF76FF

    public override void Start()
    {
        glowingScript.enabled = false;
        recoveryCycleWaitTime = GameManager.Instance.settings.gasStationSingleRecoveryCycleWaitTime;
    }

    private void OnTriggerStay(Collider col)
    {
        racer = col.transform.parent.GetComponent<IRacer>();
        if (racer != null)
        {
            if (racer.Armor.CurrentValue == racer.Armor.valueMax)
            {
                return;  //No need to shop
            }

            if (racer.RigidBodySpeedKmH < 1 && repairing == false)
            {
                repairing = true;
                StartCoroutine(Shop(col.transform.parent.GetComponent<IRacer>()));
            }
        }
    }

    IEnumerator Shop(IRacer racer)
    {
        startTimer = recoveryCycleWaitTime;
        while (racer.RigidBodySpeedKmH < 1)
        {
            startTimer -= Time.deltaTime;

            if (startTimer < 0)
            {
                if (racer.Armor.CurrentValue == racer.Armor.valueMax)
                {
                    glowingScript.enabled = false;
                    break;
                }
                glowingScript.enabled = true;
                FixSome(racer);
                FillAllAmmo(racer);
                startTimer = recoveryCycleWaitTime;
            }
        
            yield return null;
        }
        repairing = false;
    }

    public void AddSomeFuel()
    {

    }

    public void FillAllAmmo(IRacer racer)
    {
        racer.Equipment.FullAmmoToEnabledAbilities();
        HUD.instance.DisplayNotification("Ammo filled!");
    }

    /// <summary>
    ///  Repairs 10 percent of car's max armor for each 2 seconds
    /// </summary>
    /// <param name="racer"></param>
    public void FixSome(IRacer racer)
    {
        if (!racer.IsAIDriving)
        {
            racer.Armor.CurrentValue += racer.Armor.valueMax * 0.2f;

            HUD.instance.UpdateTemperature();
            Debug.Log(racer.Armor.CurrentValue);

            if (racer.Armor.CurrentValue == racer.Armor.valueMax)
            {
                glowingScript.enabled = false;
                HUD.instance.DisplayNotification("Done!");
            }
        } 
    }
}
