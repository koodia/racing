using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour //create some baseclass, basecar should use this as well
{
    public ItemPrefab itemPrefabName;
    public Effect[] effects;
    private Rigidbody rigid;
    private IRacer racer;
    public bool isAbility;

    void Start()
    {
        rigid = gameObject.GetComponent<Rigidbody>();
    }

    public void ActivateEffect()
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] == Effect.Explosion)
            {
                Debug.Log("Gaboooooooom!");

            }
            this.gameObject.SetActive(false);
        }
    }

    public void ActivateRigidBody(Transform newParent)
    {
        rigid.isKinematic = false;
        gameObject.transform.parent = newParent; //the nearest part of the map
    }

    private void OnCollisionEnter(Collision col)
    {
        racer = col.transform.GetComponent<IRacer>();
        if (racer != null && isAbility)
        {
            ActivateEffect();
            racer.Equipment.AddAbility(itemPrefabName);
            SetItselfToPool();

        }
        else
        {
            ActivateEffect();
        }

        //TODO: Put to gameObjectPool
        //gameObject.SetActive(false);
    }

    private void SetItselfToPool()
    {
        gameObject.SetActive(false);
        //pool++
    }
}

