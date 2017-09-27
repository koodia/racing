using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Rewrite the class. Read the settings from file or something. This list is going to be long, I mean LONG!
public static class CarData
{
    public static CarSettings GetCarSettings(CarCategory category, bool isAIDriven)
    {
        CarSettings settings = new CarSettings(category, isAIDriven);
        return settings;
    }

    /// <summary>
    /// Creates a playable vehicle from any vehicle prefab 
    /// </summary>
    public static void ModifyAIPrefabToPlayable(GameObject go, bool automaticTransmission, int playerId)
    {
        PlayerController player = go.AddComponent<PlayerController>();
        AIRacerController orginal = go.transform.GetComponent<AIRacerController>();
        player.playerId = playerId;

        //TODO: player = new PlayerController(playerNumber);
        player.isAIDriving = false;
        if (go.transform.GetComponent<AIRacerController>().textureNormal != null) //skipt these for now
        {
            player.textureNormal = orginal.textureNormal;
            player.textureBreaking = orginal.textureBreaking;
            player.materialLights = orginal.materialLights;
        }

        player.carCategory = orginal.carCategory; //probably useless
        CarSettings carSettings = CarData.GetCarSettings(orginal.carCategory, false);
        player.style = orginal.style;
        player.wheelFL = orginal.wheelFL;
        player.wheelFR = orginal.wheelFR;
        player.wheelRL = orginal.wheelRL;
        player.wheelRR = orginal.wheelRR;
        player.wheelType = orginal.wheelType;
        player.poweringWheels = orginal.poweringWheels;
        player.carRenderer = orginal.carRenderer;
        player.visualsObjectInHierarchy = orginal.visualsObjectInHierarchy;
        //This could be easier, fine for now

        player.specs = carSettings.specs;
        player.movement = new PlayerControls(player, new Drivetrain(carSettings.motorTorqueMax, 7000, false, carSettings.numberOfGears));
        player.controlNobs = new ControlNobs(player);
        player.fixedCamera = new FixedCamera(player.style.prefabName);
        player.Armor = new Armor(carSettings.specs.armor);
        player.maxBrakeTorgue = carSettings.maxBrakeTorgue;

        if (player.style.prefabName != CarPrefabName.Ferrari_Prefab)
        {
            Camera.main.transform.parent = go.transform;
            player.Armor = new Armor(carSettings.specs.armor);
            player.RefreshRoadObject();//TEST
        }
        else     //TODO: remove this carbage when you fix your car
        {
            Camera.main.transform.parent = go.transform.GetChild(0).transform.Find("Body");
            player.RefreshRoadObject();//TEST
        }
        Camera.main.farClipPlane = VideoSettings.CameraFarPlane;
        player.fixedCamera.ChangeCameraView(CameraView.ThirdPerson); //ThirdPerson is the default
        go.name = "Racer " + (playerId + 1) + "(Player)";
        //player.equipment = player.gameObject.AddComponent<Equipment>();
        //player.equipment.Init(player.specs.weaponSlots);
        //player.Racer = new Racer("Lakupekka", "FU-P0L1C3", false);

        //and lastly remove useless stuff:
        orginal.enabled = false;
        GameObject.Destroy(orginal); //Removes the RacerController, we dont need it anymore
        go.SetActive(false); //nessaccary?
        go.SetActive(true); //... heck, lets do it anyway
        go.GetComponent<BaseCar>().status.isEngineOn = false; //Player cannot move before the go-launch

        Debug.Assert(player.maxBrakeTorgue != 0, "maxBrakeTorgue  0 !!");

    }

    public static void PopulateCarSettings(GameObject obj, CarSettings settings)
    {
        BaseCar cntrl = obj.GetComponent<BaseCar>();

        cntrl.maxBrakeTorgue = settings.maxBrakeTorgue;
        cntrl.TopSpeed = settings.topSpeed;     //Limit later: GameSettings.maxSpeed;
        cntrl.maxSteerAngle = settings.maxSteerAngle;
        cntrl.motorTorqueMax = settings.motorTorqueMax;
        cntrl.aiDriverReactionTime = settings.aiDriverReactionTime;
        cntrl.carCategory = settings.category;
        cntrl.turnSpeed = settings.turnSpeed;
        cntrl.yawSpeed = settings.yawSpeed;
        cntrl.accelerationMetersPerSec = settings.accelerationMetersPerSec;
        cntrl.specs = settings.specs;
        cntrl.Armor = new Armor(settings.specs.armor);

        //At the moment we dont know which prefabCar this is
        //cntrl.cargo.SetCarsSpesificCargoSettings(cntrl.style.prefabName);

        //TODO: cntrl.maxRPM = settings.maxRPM;

        Debug.Assert(settings.specs.armor != ArmorLevel.NotSet);
        Debug.Assert(settings.specs.torgue != 0);


        if (cntrl.rigid == null) //TODO:  this is chewing gum
		{
			cntrl.rigid = obj.GetComponent<Rigidbody>();
		}

		if (cntrl.rigid != null) //TODO: how to fix 
		{
			cntrl.rigid.mass = settings.mass;
			cntrl.rigid.centerOfMass  = new Vector3(0,-1.5f,0); //Hope this does some magic. Same for all cars
		}
      
        cntrl.isAIDriving = settings.isAIDriven;
        cntrl.wheelType = settings.wheelType;

        if (!cntrl.isAIDriving)
        {
            PlayerController playrCntrl = obj.GetComponent<PlayerController>();
            if (playrCntrl == null)
            {
                Debug.Log("Could not find PlayerController when populating car settings");      
            }
            playrCntrl.movement.Drivetrain = new Drivetrain(settings.motorTorqueMax, 7000, true, settings.numberOfGears); //SÄTÖ
            //playrCntrl.Racer = new Racer("Pekka Pelaaja", "B4SS-F03VA", false);
            cntrl.Armor = new Armor(settings.specs.armor);
        }

        //Set each wheel
        WheelCollider[] wheels = obj.GetComponentsInChildren<WheelCollider>();
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].tag == "RearWheel")
            {
                wheels[i].sidewaysFriction = settings.rearWheelsSidewaysFrictionCurve;
                wheels[i].forwardFriction = settings.rearWheelsForwardFrictionCurve;
                wheels[i].suspensionSpring = settings.rearSuspensionSpring;
            }
            else if (wheels[i].tag == "FrontWheel")
            {
                wheels[i].sidewaysFriction = settings.frontWheelsSidewaysFrictionCurve;
                wheels[i].forwardFriction = settings.frontWheelsForwardFrictionCurve;
                wheels[i].suspensionSpring = settings.frontSuspensionSpring;
            }
            else
            {
             //   Debug.Log("Wheel has no tag! Object name:" + obj.name);
            }
        }


    }
}

/// <summary>
/// Some class to hold the predefined settings of each car type. Temporary implementation
/// </summary>
public class CarSettings //Settings for musclecar
{
    public WheelFrictionCurve rearWheelsForwardFrictionCurve = new WheelFrictionCurve();
    public WheelFrictionCurve rearWheelsSidewaysFrictionCurve = new WheelFrictionCurve();
    public WheelFrictionCurve frontWheelsForwardFrictionCurve = new WheelFrictionCurve();
    public WheelFrictionCurve frontWheelsSidewaysFrictionCurve = new WheelFrictionCurve();
    public JointSpring frontSuspensionSpring = new JointSpring();
    public JointSpring rearSuspensionSpring = new JointSpring();

    public WheelType wheelType;
    //[Header("Power")]
    public float motorTorqueMax;
    public float topSpeed;
    public int numberOfGears;
    public int maxRPM;
    public float accelerationMetersPerSec;

    //[Header("Steering & Handling")]
    public float maxSteerAngle;
    public float turnSpeed;
    public float distanceToSwitchNode;
    public float targetSteerAngle;
    public float aiDriverReactionTime;
  

    //[Header("Braking")]
    public float maxBrakeTorgue;

    //[Header("Sensors")]
    public float sensorLength;
    public float frontSideSensorPosX;
    public float frontSensorAngle;
    public Vector3 frontSensorPosition;
    public float frontTopSensorHeight;

    //[Header("Physics")]
    public float simpleDistToGround;
    public float yawSpeed;
    public float mass;
    public Vector3 centerOfMass;

    /*TODO: public Vector2 weightDistribution;
    A car with "50/50" weight distribution will understeer on initial corner entry.To avoid this problem, 
    sports and racing cars often have a more rearward weight distribution.In the case of pure racing cars, 
    this is typically between "40/60" and "35/65".[citation needed] This gives the front tires an advantage
    in overcoming the car's moment of inertia (yaw angular inertia), thus reducing corner-entry understeer.
    */
  
    public CarCategory category;
    public CarSpecs specs;
    public Cargo cargo;
    public bool isAIDriven;

    public Vector3[] weaponsPlaces;

    /// <summary>
    /// General settings for each vehicle category
    /// </summary>
    /// <param name="category"></param>
    /// <param name="isAIDriven"></param>
    public CarSettings(CarCategory category, bool isAIDriven)
    {
        if (category == CarCategory.Muscle)
        {
            CamaroSettings(isAIDriven);
        }
        else if (category == CarCategory.Racing)
        {
            FormulaSettings(isAIDriven);
        }
        else if (category == CarCategory.HeavyDuty)
        {
            CargoTruckSettings(isAIDriven);
        }
        else if (category == CarCategory.Guns)
        {
            DodgeRamSettings(isAIDriven);
        }
        else if (category == CarCategory.CargoCarrier)
        {
            DodgeRamSettings(isAIDriven);
           // SetHarderSpringForCargo();
        }
        else
        {
            throw new System.NotImplementedException("CarCategory:" + category);
        }

        this.isAIDriven = isAIDriven;
        this.category = category;
        this.accelerationMetersPerSec = 1f; //1000meter a sec
    }
}


