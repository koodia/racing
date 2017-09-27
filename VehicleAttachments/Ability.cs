using UnityEngine;

[System.Serializable]
public class Ability
{
    private bool instantUse = false;
    public bool enabled = false;
    [HideInInspector] public GameObject weaponPrefab;
    [HideInInspector] public Vector3 projectileSpawnPos;
    [HideInInspector] public string projectilePrefab;
    [HideInInspector] public float fireRate;
    [HideInInspector] public float projectileSpeed;
    [SerializeField]
    public int ammoCount;
    public int ammoCapacity;
    public int ammoClipSize;
    public float aliveTime;

    public Ability(bool enable, bool instantUse, string projectilePrefab, int ammoClipSize, float cooldownTime, float projectileSpeed)
    {
        this.enabled = enable;
        this.instantUse = instantUse;
        this.projectilePrefab = projectilePrefab;
        this.aliveTime = 6;
        this.ammoClipSize = ammoClipSize;
        this.fireRate = cooldownTime;
        this.projectileSpeed = projectileSpeed;
    }

    public Ability(bool enable, bool instantUse)
    {
        this.enabled = enable;
        this.instantUse = instantUse;
        this.aliveTime = 6; //TODO
    }

    //TODO: rewrite
    public void CreateFixedProjectileStartPos()
    {
        projectileSpawnPos = new Vector3(weaponPrefab.transform.position.x, weaponPrefab.transform.position.y, weaponPrefab.transform.position.z);
    }

    public void EnableAbility(bool show)
    {
        this.enabled = show;
        if (weaponPrefab != null) //temporaryhack
        {
            this.weaponPrefab.SetActive(show);
        }
    }

    /// <summary>
    /// Adds one clip if optional parameter is 0
    /// </summary>
    /// <param name="ammo"></param>
    public void AddOneAmmo(int ammo = 0)
    {
        if (ammo != 0)
        {
            this.ammoCount += ammo;
        }
        else
        {
            this.ammoCount += ammoClipSize;
        }
    }

    public virtual void ActivateOrFire()
    {
    }

    public virtual void Destroy()
    {
    }
}