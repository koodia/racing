using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equipment : MyMonoBase
{
    [SerializeField]
    public Ability[] abilities = new Ability[20];
    [SerializeField]
    public int currentSelection;
    float timeGoneFromPreviousFire;

    public override void Awake()
    {
        //TODO NoneClass
        abilities[0] = new Missile(false, false, AbilityPrefab.MissileBullet_Prefab.ToString(), 3, 3, 600);
        abilities[1] = new MachineGun(false, false, AbilityPrefab.MachineGunBullet_Prefab.ToString(), 70, 0.5f, 800);

        //TODO:
        abilities[2] = new MachineGunDual(false, false, AbilityPrefab.MachineGunBullet_Prefab.ToString(), 100, 0.5f, 0); //duel mode
        abilities[3] = new Nitro(false, false, AbilityPrefab.ToDO.ToString(), 10, 10f, 0);

        //These are enum PowerUps, which might not be end up in the list, but make an implemantation that supports this
        abilities[4] = new NitroBoost(true, true);
        abilities[5] = new QualityRoad(true, true);
        //  powerUps.Add(PowerUpName.NitroBoost, new NitroBoost(true, false));
        //  powerUps.Add(PowerUpName.QualityRoad, new NitroBoost(true, false));

        currentSelection = 0;
    }

    public void Init(int weaponSlots)
    {
        //Finds the Equipment TransformObject, also creates the fixed projectile spawnposition
        var equipmentObject = gameObject.transform.GetChild(0).transform.Find("Equipment");
        Debug.Assert(equipmentObject != null, "Could not find Equipment transform from a vehicle");
        if (weaponSlots > 0)
        {
            FindMissile(equipmentObject);
            FindMachineGun(equipmentObject);
            FindMachineGunTwo(equipmentObject);
        }
    }

    #region some bs
    //bs
    private void FindMissile(Transform equipmentObject)
    {
        abilities[0].weaponPrefab = equipmentObject.Find("Missile").gameObject;
        //abilities[0].CreateFixedProjectileStartPos(); //TODO
        abilities[0].EnableAbility(false);
    }
    private void FindMachineGun(Transform equipmentObject)
    {
        abilities[1].weaponPrefab = equipmentObject.Find("MachineGun").gameObject;
        //abilities[1].CreateFixedProjectileStartPos(); //TODO
        abilities[1].EnableAbility(false);
    }
    private void FindMachineGunTwo(Transform equipmentObject)
    {
        try
        {
            abilities[2].weaponPrefab = equipmentObject.Find("MachineGun2").gameObject;    //Some vehicles have multiple 
            abilities[2].EnableAbility(false);
        }
        catch (Exception e)
        {
            //TODO, create nicer solution later
        }
    }

    #endregion

    public void CycleSelection(bool nextOrPrevious)
    {
        int i = currentSelection;
        if (nextOrPrevious)
        {
            Debug.Log("Trying to cycle next");
            if (i + 1 > 20)
            {
                i = -1;
            }
            else
            {
                i++;
            }

            if (abilities[i] != null)
            { 
                if (abilities[i].enabled)
                {
                    currentSelection++;
                    Focus(abilities[currentSelection]);
                }
            }
        }
        else
        {
            Debug.Log("Trying to cycle previous");
            if (i - 1 < 0)
            {
                i = abilities.Length;
            }
            else
            {
                i--;
            }

            if (abilities[i] != null)
            {

                if (abilities[i].enabled)
                {
                    currentSelection--;
                    Focus(abilities[currentSelection]);
                }
            }
        }
    }

    int tempInt;
    public void AddAbility(ItemPrefab itemName)
    {
        tempInt = -1;
        tempInt = GetIndexByName(itemName);
        Debug.Assert(tempInt != -1,"tempInt is -1");
        abilities[tempInt].EnableAbility(true);
        abilities[tempInt].AddOneAmmo();
    }

    /// <summary>
    /// Temporary solution
    /// </summary>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public int GetIndexByName(ItemPrefab itemName)
    {
        switch (itemName)
        {
            case ItemPrefab.MissileCrate_Prefab:
                return 0;
            case ItemPrefab.MachineGunCrate_Prefab:
                if (abilities[1].enabled == true)
                {
                    return 2; //activates duals
                }
                else
                {
                    return 1;
                }
            case ItemPrefab.NitroCrate_Prefan:
                    return 3;
        }
        throw new NotImplementedException(":" + itemName);
    }

    public void ActivateAllAbilities()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] == null)
            { continue; }

            abilities[i].EnableAbility(true);
            abilities[i].AddOneAmmo();
        }
    }

    public void UseNitro()
    {
        UseTargetSelection(3);
    }

    GameObject bullet;
    float aliveTime;
    float projectileSpeed;
    public void UseTargetSelection(int abilityIndex)
    {
        if (abilities[abilityIndex].projectilePrefab != "ToDO") //Some of the prefabs are not ready
        {
            if (abilityIndex == 3)
            {
                if (timeGoneFromPreviousFire < 0)
                {
                    abilities[3].ActivateOrFire();
                    timeGoneFromPreviousFire = abilities[3].fireRate;
                }
            }
            else
            {
                IfProjectileAbility(abilityIndex);
            }
        }
    }

    private void IfProjectileAbility(int abilityIndex)
    {
        if (abilities[abilityIndex].enabled)
        {
            if (timeGoneFromPreviousFire < 0)
            {
                //Create the Bullet from the Bullet Prefab
                bullet = (GameObject)Instantiate(
                PrefabManager.Instance.GetPrefab(PrefabType.Item,
                abilities[abilityIndex].projectilePrefab),
                new Vector3(abilities[abilityIndex].weaponPrefab.transform.position.x + 20, //for machinegun testing
                abilities[abilityIndex].weaponPrefab.transform.position.y,
                abilities[abilityIndex].weaponPrefab.transform.position.z), //TODO create as quoternion? = abilities[currentSelection].projectileSpawn);
                transform.rotation);
                aliveTime = abilities[abilityIndex].aliveTime;
                projectileSpeed = abilities[abilityIndex].projectileSpeed;
                StartCoroutine(MoveBulletSeconds());
                abilities[abilityIndex].ammoCount--;

                //timeGoneFromPreviousFire = abilities[currentSelection].cooldownTime;
                if (abilities[abilityIndex].ammoCount < 1)
                {
                    abilities[abilityIndex].EnableAbility(false);
                    //CycleSelection(false); //goes to previous selection
                    Debug.Log("No ammo left!");
                }
                else
                {
                    Debug.Log("Fire!");
                }

                timeGoneFromPreviousFire = abilities[abilityIndex].fireRate;
            }
        }
    }

    void FixedUpdate()
    {
        aliveTime -= Time.deltaTime;
        timeGoneFromPreviousFire -= Time.deltaTime;
    }

    private IEnumerator MoveBulletSeconds()
    {
        while (aliveTime > 0)
        {
            bullet.GetComponent<Rigidbody>().MovePosition(bullet.transform.position + transform.forward * projectileSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(bullet, aliveTime);
    }

    private void Focus(Ability itemList)
    {
        Debug.Log("Current ability:" + abilities[currentSelection].GetType().Name); //baad
        //Update HUD
    }

    //public bool HasAllEnabledGunsAmmo()
    //{
    //    for (int i = 0; i < abilities.Length; i++)
    //    {
    //        if (abilities[i].enabled && abilities[i].ammoCount < abilities[i].ammoCapacity);
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    public void FullAmmoToEnabledAbilities()
    {
       for (int i = 0; i < abilities.Length; i++)
        {
        if (abilities[i] != null)
        {
            if (abilities[i].enabled)
            {
                abilities[i].ammoCount = abilities[i].ammoCapacity;
            }
        }
        }
    }

    //public GameObject GetCorrespondingPrefab(AbilityPrefabName prefabName)
    //{
    //    switch (prefabName)
    //    {
    //        case AbilityPrefabName.MachineGun_Prefab:
    //            return this.machineGunBulletPrefab;
    //        case AbilityPrefabName.Missile_Prefab:
    //            return this.missileBulletPrefab;
    //        default: throw new Exception("dfasfs?!!");
    //    }
    //}
}
    

    public class MachineGunDual : Ability
    {
        public MachineGunDual(bool enable, bool instantUse, string projectilePrefab, int ammoClipSize, float cooldownTime, float projectileSpeed) : base(enable, instantUse, projectilePrefab,  ammoClipSize, cooldownTime, projectileSpeed)
        {
        }
    }

    public class MachineGun : Ability
    {
        public MachineGun(bool enable, bool instantUse, string projectilePrefab, int ammoClipSize, float cooldownTime, float projectileSpeed) : base(enable, instantUse, projectilePrefab, ammoClipSize, cooldownTime, projectileSpeed)
        {
        }

        //public override void GetDataForActivateOrFire(ref float timeGoneFromPreviousFire)
        //{
        //    if (timeGoneFromPreviousFire < 0)
        //    {
        //        // Create the Bullet from the Bullet Prefab
        //        //var bullet = (GameObject)Instantiate(
        //        //    projectilePrefab,
        //        //    projectileSpawn.position,
        //        //    projectileSpawn.rotation);

        //        //// Add velocity to the bullet
        //        //bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

        //        //// Destroy the bullet after 2 seconds
        //        //Destroy(bullet, 2.0f);
        //        timeGoneFromPreviousFire = cooldownTime;
        //        if (ammoCount.Length < 1)
        //        {
        //            enabled = false;
        //        }
        //    }
        //    //else
        //    //{
        //    //    timeGoneFromPreviousFire -= Time.deltaTime;
        //    //}
        //}
    }

    public class Missile : Ability
    {
        public Missile(bool enable, bool instantUse, string projectilePrefab, int ammoClipSize, float cooldownTime, float projectileSpeed) 
        : base(enable, instantUse, projectilePrefab, ammoClipSize, cooldownTime, projectileSpeed)
        {
        }

        public override void ActivateOrFire()
        {
            //if (timeGoneFromPreviousFire < 0)
            //{
            //    // Create the Bullet from the Bullet Prefab
            //    var bullet = (GameObject)Instantiate(
            //    projectilePrefab,
            //    projectileSpawn.position,
            //    projectileSpawn.rotation);

            //    // Add velocity to the bullet
            //    bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

            //    Destroy(bullet, 2.0f);
            //    if (ammoCount.Length < 1)
            //    {
            //        enabled = false;
            //    }

            //    timeGoneFromPreviousFire = cooldownTime;
            //}
        }
    }

    public class Nitro : Ability
    {
        public Nitro(bool enable, bool instantUse, string projectilePrefab, int ammoClipSize, float cooldownTime, float projectileSpeed) 
        : base(enable, instantUse, projectilePrefab, ammoClipSize, cooldownTime, projectileSpeed)
        {
        }

        public override void ActivateOrFire()
        {
                //TODO: call Drivetrain nitro!
             
         }
    }


    public class NitroBoost : Ability
    {
        public NitroBoost(bool enable, bool instantUse) : base(enable, instantUse)
        {
        }

        public override void ActivateOrFire()
        {
        }
    }

    public class QualityRoad : Ability
    {
        public QualityRoad(bool enable, bool instantUse) : base(enable, instantUse)
        {
        }

        public override void ActivateOrFire()
        {
        }
    }