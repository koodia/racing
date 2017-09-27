using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficVehicleCreator //<Singleton>
{
    private float tempColor;
    private Color newColor;
    private Array values = Enum.GetValues(typeof(CarPrefabName));

    public VehicleStyle PickRandomVehicle()
    {
        CarPrefabName prefabName = (CarPrefabName)values.GetValue(My.rand.Next(values.Length - 1)); //Minus 1, no ferrari in traffic!
        Color color = CreateProperColorForCar(false);
        DriveStyle style = RandDrivingStyle();
        VehicleStyle randomStyle = new VehicleStyle(prefabName, color, style);
     
        return randomStyle;
    }

    public VehicleStyle PickVehicle(CarPrefabName prefabName)
    {
        Color color = CreateProperColorForCar(false);
        DriveStyle style = RandDrivingStyle();
        VehicleStyle randomStyle = new VehicleStyle(prefabName, color, style);

        return randomStyle;
    }

    /// <summary>
    /// Currently 2 different racers prefabs. More later
    /// </summary>
    /// <returns></returns>
    public VehicleStyle PickRandomRaceVehicleAndStyle()
    {
        CarPrefabName[] values = new CarPrefabName[3];
        values[0] = CarPrefabName.Camaro_Prefab;
        values[1] = CarPrefabName.DodgeRam_Prefab;
        values[2] = CarPrefabName.Ferrari_Prefab;

        Color color = CreateProperColorForCar(true);
        DriveStyle style = RandDrivingStyle();
        VehicleStyle randomStyle = new VehicleStyle((CarPrefabName)values.GetValue(My.rand.Next(values.Length)), color, style);

        return randomStyle;
    }


    public void DecorateCarWithStyle(GameObject obj, VehicleStyle style)
    {
        if (!style.isSpecialColored) //Dont paint the special vehicles
        { ChangeColor(obj.transform.GetComponent<BaseCar>().carRenderer, style.color); }

        obj.GetComponent<BaseCar>().style = style;
    }

    /// <summary>
    /// Colors for traffic. Rubbish implementation, but fun to see some colors  https://en.wikipedia.org/wiki/Car_colour_popularity
    /// 40% white
    /// 20% gray to black
    /// 10% blue
    /// 10& anything but very dark
    /// 5% anything 
    /// for racers very strong or bright
    /// </summary>
    /// <param name="isNotRacer"></param>
    /// <param name="isSpecialVehicle"></param>
    /// <returns></returns>
    public Color CreateProperColorForCar(bool isRacer)
    {
        int num = UnityEngine.Random.Range(1, 10);

        if (isRacer)
        num = 10;

        switch (num)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                //whites
                tempColor = UnityEngine.Random.Range(0.2f, 0.47f);
                newColor = new Color(tempColor, tempColor, tempColor, 1f);
                break;
            case 5:
            case 6:
                //blacks & gray
                tempColor = UnityEngine.Random.Range(0.47f, 1);
                newColor = new Color(tempColor, tempColor, tempColor, 1f);
                break;
            case 7:
            case 8:
                //browns and caramel
                float r = UnityEngine.Random.Range(0.82f, 1f); 
                float g = UnityEngine.Random.Range(0.7f, 1f);
                tempColor = UnityEngine.Random.Range(0.54f, 1f);
                newColor = new Color(r, g, tempColor);
                break;

            case 9:
                //blues. Also TODO lavender and light blues
                tempColor = UnityEngine.Random.Range(0, 1f);
                newColor = new Color(0, 0, tempColor, 1f);
                break;
            case 10:
                //any
                newColor = new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), 1);
                break;
            default: throw new Exception("You messed your coin tos method");
        }

        return newColor;
    }

    public void ChangeColor(Renderer rend, Color color)
    {
        //TODO: Remove the catch
        try
        {
            if (rend.gameObject.transform.root.name.Contains("OldBeetle_Prefab")) //TODO: short or reference the body paint material
            {
                rend.materials[0].color = color;
            }
            else
            {
                rend.materials[1].color = color;
            }  
        }
        catch (Exception e)
        {
            Debug.Log(e.Message.ToString());
        }
    }

    public void ChangeDriveStyle()
    {

    }

    private  DriveStyle RandDrivingStyle()
    {
        Array values = Enum.GetValues(typeof(DriveStyle));
        return (DriveStyle)values.GetValue(My.rand.Next(values.Length));
    }
}

    [System.Serializable]
    public class VehicleStyle
    {
        public string carName;
        public CarPrefabName prefabName;
        public string racerPrefabTempThing; //temporary thing until AIRacer can be created from TrafficCar
        [HideInInspector] public Color color;
        //public string colorRange;
        public DriveStyle style;
        public bool isSpecialColored;
        public float vehicleLength; //a precasion used by vehicleSpawner
        public bool hasCargoSpace;

        public VehicleStyle(CarPrefabName prefabName, Color color, DriveStyle style)
        {
            carName = prefabName.ToString();
            this.prefabName = prefabName;
            this.color = color;
            this.style = style;

            if (prefabName == CarPrefabName.CargoTruck_Prefab || prefabName == CarPrefabName.MiniCargoTruck_Prefab || prefabName == CarPrefabName.DodgeRam_Prefab || prefabName == CarPrefabName.Ferrari_Prefab)
            {
                this.isSpecialColored = true; //checkFromPrefabName or something like that
            }

            //Temp haxing
            if (prefabName == CarPrefabName.Camaro_Prefab)
            {
                racerPrefabTempThing = "RacerCamaro_Prefab";
            }
            else if (prefabName == CarPrefabName.DodgeRam_Prefab)
            {
                racerPrefabTempThing = "RacerDodgeRam_Prefab";
            }
            else if (prefabName == CarPrefabName.Ferrari_Prefab)
            {
                racerPrefabTempThing = CarPrefabName.Ferrari_Prefab.ToString(); //notice the difference
            }

            if (prefabName == CarPrefabName.MiniCargoTruck_Prefab | prefabName == CarPrefabName.L200Pickup_Prefab)
            {
                hasCargoSpace = true;
            }
    }

    /// <summary>
    /// The pivot point might change depending on the vehicle length and the cargo length
    /// </summary>
    /// <returns></returns>
    public float AddDistanceIfNeeded()
    {
        if (this.prefabName == CarPrefabName.CargoTruck_Prefab)
        {
            return 100;
        }
        else if (this.prefabName == CarPrefabName.Metrobus_Prefab)
        {
            return 40;
        }

        return 0;
    }

    /// <summary>
    /// Kinda temp solution, adds some distance, a car is instantiated
    /// </summary>
    /// <param name="currentInstantiationDistance"></param>
    /// <returns></returns>
    public float AddDistanceIfNeeded( ref float currentInstantiationDistance)
    {
            if (this.prefabName == CarPrefabName.CargoTruck_Prefab)
            {
               
                currentInstantiationDistance += 150; //just some estimation
                return 150;  //just some estimation
        }

            return 0;
    }


}





    