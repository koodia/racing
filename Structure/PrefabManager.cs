using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum PrefabType
{
    None = 0,
    PowerUp,
    Item,
    Vehicle,
}

public sealed class PrefabManager : Singleton<PrefabManager>
{
    [SerializeField]
    public GameObject[] powerUpPrefabs = new GameObject[3];
    [SerializeField]
    public GameObject[] vehiclePrefabs = new GameObject[3];
    [SerializeField]
    public GameObject[] itemPrefabs = new GameObject[3];

    public Dictionary<PrefabType, GameObject[]> prefabLibraries = new Dictionary<PrefabType, GameObject[]>();
    public void InitPrefabManager()
    {
        prefabLibraries.Add(PrefabType.Vehicle, vehiclePrefabs);
        prefabLibraries.Add(PrefabType.Item, itemPrefabs);
        prefabLibraries.Add(PrefabType.PowerUp, powerUpPrefabs);
    }

    public override void Awake()
    {
        base.Awake();
        InitPrefabManager();
    }

    public GameObject GetPrefab(PrefabType prefabType, string name)
    {
        foreach (var data in prefabLibraries)
        {
            if (data.Key == prefabType)
            {
                foreach (GameObject obj in data.Value)
                {
                    if (obj.name == name)
                    {
                        return obj;
                    }
                }
            }
        }

        return null;
    }
}
    
