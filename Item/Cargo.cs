using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo
{ 
    public int numberOfCargo;
    Item[] items;
    public Vector3[] cargoPositions;

    public void SetCarsSpesificCargoSettings(CarPrefabName prefabName)
    {
        switch (prefabName)
        {
            case CarPrefabName.MiniCargoTruck_Prefab:

                this.cargoPositions = new Vector3[]
                {
                    //TODO: Fix the model
                   new Vector3(1,  4.15f, - 6), //front left
                   new Vector3(6,  4.15f, - 6), //front right
                   new Vector3(1,  4.15f, - 12), //back left
                   new Vector3(6,  4.15f, - 12)
                };
                this.items = new Item[4];
                numberOfCargo = 0;
                break;

            case CarPrefabName.L200Pickup_Prefab:

                this.cargoPositions = new Vector3[]
                {
                   new Vector3(0,  4.15f, - 8), //single at middle
                };
                this.items = new Item[1];
                numberOfCargo = 0;
                break;

            default: //already Vectro3.zero
                break;
        }
    }

    public void AddCargoItem(Item item)
    {
        this.items[numberOfCargo] = item;
        numberOfCargo++;
    }

    public void CheckCollisionVelocity(Transform player, GameObject cargoCar)
    {
        //if (player.GetComponent<AIBaseCar>().rigdig.velocity > cargoCar.GetComponent<AIBaseCar>().rigdig.velocity)
        //{
        //    ReleaseSingleItem();
        //}
    }

    public Cargo(int cargoMax, Vector3[] cargoPositions = null)
    {
        this.items = new Item[cargoMax];
        if (cargoPositions == null)
        {
            cargoPositions = new Vector3[] { Vector3.zero };
        }
        else
        {
            this.cargoPositions = cargoPositions;
        }
    }

    public void ReleaseSingleItem(float carPosZ)
    {
      
        if (numberOfCargo > 0)
        {
            try  // road part might be disabled and the transform is null, so lets shelve the items somewhere for now.
            {
                items[numberOfCargo - 1].ActivateRigidBody(RoadManager.instance.GetClosestRoadPart(carPosZ));
            }
            catch (NullReferenceException e)
            {
                items[numberOfCargo - 1].ActivateRigidBody(RoadManager.instance.gameObject.transform);
            }
            numberOfCargo--;
        }
    }

    public void ReleaseWholeCargo(float carPosZ)
    {
        if (numberOfCargo > 0)
        {
            for (int i = 0; i < numberOfCargo; i++)
            {
                try  // road part might be disabled and the transform is null, so lets shelve the items somewhere for now.
                {
                    items[i].ActivateRigidBody(RoadManager.instance.GetClosestRoadPart(carPosZ));
                }
                catch (NullReferenceException e)
                {
                    items[i].ActivateRigidBody(RoadManager.instance.gameObject.transform);
                }
            }
            numberOfCargo = 0;
        }
       
    }
}
