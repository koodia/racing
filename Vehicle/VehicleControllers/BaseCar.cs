
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;



//********************************** A hack
public class ReadOnlyAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
//**********************************

[RequireComponent(typeof(PathController))]
[System.Serializable]
public abstract class BaseCar : RoadObject
{
    public int temporaryWorkAroundRacerId;

    [Header("MUST BE SET IN INSPECTOR")]
    //[SerializeField]
    public CarCategory carCategory; //TODO:put this to VehicleStyle class if you can
    public VehicleStyle style; //TODO: Get rid off. Class is only for debugging:

    [Header("Set by programmatically")]
    [ReadOnly] public CarSpecs specs;

    public Sensors sensors;
    public RoadNode currentRoadNode = new RoadNode();
    [HideInInspector] public PathController pathController;
    [Header("Visuals")]
    public Texture2D textureNormal;
    public Texture2D textureBreaking;
    public Renderer carRenderer;
    //public Material sharedMaterialLights;
    [Header("taillights texture")]
    public Material materialLights;
    public GameObject visualsObjectInHierarchy;

    public DrivingStatus status;
    public RaceStatus raceStatus;
    [HideInInspector] private const int maximumNodeJumpWhenSwitchingLane = 3;

    [Header("Power")]
    [HideInInspector]
    public float accelerationMetersPerSec;
    [HideInInspector] public float motorTorqueMax;
    [ReadOnlyAttribute] public float currentSpeed;
    [SerializeField]
    public float rigidBodySpeedKmH; //rigdig.velocity.magnitude * 3.6f
    public float RigidBodySpeedKmH
    {
        get { return rigidBodySpeedKmH; }
        set { rigidBodySpeedKmH = value; }
    }
    public float CurrentSpeed //Player uses different formula, because the drivetrain. For AI cars the speed is calculated by formula: 2 * Mathf.PI * wheelFL.radius * 60 / 1000 * wheelRL.rpm;
    {
        get { return currentSpeed; }
        set { currentSpeed = value; }
    }

    public float topSpeed;
    public float TopSpeed
    {
        get { return topSpeed; }
        set { topSpeed = value; }
    }

    [SerializeField]
    public Armor Armor { get; set; }
    [Header("Steering & Handling")]
    public float maxSteerAngle;
    public float turnSpeed;
    [HideInInspector] public const float distanceToSwitchNode = 50f; //This is kinda crucial. Effects the path finding accuracity

    [HideInInspector] public float targetSteerAngle;
    [HideInInspector] public float aiDriverReactionTime;

    [Header("Braking")]
    [SerializeField]
    [HideInInspector] public float maxBrakeTorgue;

    [Header("Physics")]
    //[SerializeField]
    //[ReadOnly] bool isGrounded;
    [HideInInspector] public float simpleDistToGround = 4;

    //  public Rigidbody rigdig;
    [HideInInspector] public float yawSpeed;
    //[HideInInspector] public Vector3 centerOfMass;

    //precalculated values;
    public float wheelSpinningWithoutRpm;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public WheelType wheelType;
    public WheelCollider[] poweringWheels;

    //Put on separateClass
    //MISC & Temp:
    protected float lifeTime = GameSettings.deathTimer;
    protected bool isDeathTimerOn = false;
    [HideInInspector] public bool isAIDriving;
    public bool IsAIDriving
    {
        get { return isAIDriving; }
        set { isAIDriving = value; }
    }
    static public float visualRangeTimer; //Everyone uses the same timer
    public Cargo cargo = new Cargo(0);
    [SerializeField]
    public bool justKeeptStillForDebugging = false;

    //public void CalculateRigidBodySpeed()
    //{
    //    rigidBodySpeedKmH = rigdig.velocity.magnitude * 3.6f;
    //}
 

    public void Init(bool isAIDriving)
    {
        if (carCategory == CarCategory.NotSet)
        {
            throw new Exception("carCategory cannot be 'Notset' and must be set explicitly from Inspector!");
        }

        if (rigid == null) //Just a test
        {
            rigid = gameObject.GetComponentInChildren<Rigidbody>();
        }

        //visuals.materialLights = visuals.carRenderer.materials[2]; //TODO
        CarSettings carSettings = CarData.GetCarSettings(carCategory, isAIDriving);
        IntializeDrivingSettings(carSettings);
        SetCarWheelType(carSettings.wheelType);
        sensors = new Sensors(carCategory);
        PreCalculateFunctions();
    }

    public void SetCarWheelType(WheelType t)
    {
        switch (t)
        {
            case WheelType.FWD:
                poweringWheels = new WheelCollider[2];
                poweringWheels[0] = wheelFL;
                poweringWheels[1] = wheelFR;
                break;
            case WheelType.RWD:
                poweringWheels = new WheelCollider[2];
                poweringWheels[0] = wheelRL;
                poweringWheels[1] = wheelRR;
                break;
            case WheelType.AWD:
                poweringWheels = new WheelCollider[4];
                poweringWheels[0] = wheelFL;
                poweringWheels[1] = wheelFR;
                poweringWheels[2] = wheelRL;
                poweringWheels[3] = wheelRR;
                break;
            case WheelType.Custom:
                Debug.Log("Tyres need to set manually using Inspector (WheelType.Custom)");
                break;
        }
    }


    public void StartDeathCalculator()
    {
        isDeathTimerOn = true;
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
           // GameOver();
        }
    }

    /// <summary>
    /// Probably useless because of the speed in game
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    IEnumerator DisableCalculatorAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
        yield return null;
    }

    public void GameOver()
    {
        if (raceStatus.isStartingAtStartingLine) //TODO erase this check, checking if its a racer for now
        {
            print("Its game over for racer " + gameObject.name);
        }
        status.isEngineOn = false;
        raceStatus.gameOver = true;

        StartCoroutine(DisableCalculatorAfter(5));
    }

    //TODO rewrite when time
    public void IntializeDrivingSettings(CarSettings settings)
    {
        CarData.PopulateCarSettings(gameObject, settings);

		//TODO: chewing gum fixhack
		maxSteerAngle = settings.maxSteerAngle;
		turnSpeed = settings.turnSpeed;
        maxBrakeTorgue = settings.maxBrakeTorgue;

    }

    private void PreCalculateFunctions()
    {
        
        wheelSpinningWithoutRpm = 2 * Mathf.PI * wheelFL.radius * 60 / 1000;
        //wheelSpinningWithoutRpm = 2 * Mathf.PI * poweringWheels[0].radius * 60 / 1000;  //TODO REMEMBER THIS  !!poweringWheels[0].radius
    }
		
    public void LerpToSteeringAngle(float targetAngle, bool aiDriving)
    {
        wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetAngle, Time.deltaTime * turnSpeed);
        wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetAngle, Time.deltaTime * turnSpeed);
    }

    /// <summary>
    /// Experimental. Colliders have a bool isGrounded that might be useful, but it might be heavy?
    /// </summary>
    /// <returns></returns>
    public bool OnGroundSimpleVersion()
    {
        return Physics.Raycast(gameObject.transform.position, -gameObject.transform.up, simpleDistToGround);
    }

  
    public bool OnUpsideDown()
    {

        //if (transform.up.y < -0.93f)
        //{
        //    return true;
        //}

        if (Vector3.Dot(transform.up, Vector3.down) > 0)
        {
            status.boolUpsideDown = true;
            return true;
        }

        status.boolUpsideDown = false;
        return false;
    }

    public void SelfDestruct()
    {
        if (transform.position.y < -10)
        {
            status.isEngineOn = false;

            if (!raceStatus.isStartingAtStartingLine) //dont destroy racers because the position is updated at IEnumerator UpdateRacePosition()
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }

            VehicleSpawner.totalNumberOfVehicles--;
        }
    }

    #region Controls

    /// <summary>
    /// Brakes with front tyres only
    /// </summary>
    public void OnBrake()
    {
        status.isBraking = true;
        status.isReverse = false;

        for (int i = 0; i < poweringWheels.Length; i++)
        {
            poweringWheels[i].motorTorque = 0;
        }

        BrakeWithAllTires(maxBrakeTorgue);
        //visuals.materialLights.mainTexture = visuals.textureBreaking;
    }

    public void KillTorgueFromWheels()
    {
        for (int i = 0; i < poweringWheels.Length; i++)
        {
            poweringWheels[i].motorTorque = 0;
        }
    }

    public void BrakeWithAllTires(float brakingForce)
    {
        wheelFR.brakeTorque = brakingForce;
        wheelFL.brakeTorque = brakingForce;
        wheelRL.motorTorque = brakingForce; //just a test
        wheelRR.motorTorque = brakingForce; //just a test
    }


    public void OnReverse()
    {
        status.isBraking = false;
        status.isReverse = true;
        wheelFR.brakeTorque = 0;
        wheelFL.brakeTorque = 0;

        for (int i = 0; i < poweringWheels.Length; i++)
        {
            poweringWheels[i].motorTorque = -motorTorqueMax;
        }

        //wheelRL.motorTorque = -motorTorqueMax;
        //wheelRR.motorTorque = -motorTorqueMax;
        //if (materialLights.mainTexture == textureBreaking) //or equals?
        //{
        //    materialLights.mainTexture = textureNormal; //switch breaking texture to normal when reversing
        //}
    }

    public void OnThrottle()
    {
        status.isBraking = false;
        status.isReverse = false;
        BrakeWithAllTires(0);
    }

    public void OnReleaseBrake()
    {
        status.isBraking = false;
        BrakeWithAllTires(0);
        //sharedMaterialLights.mainTexture = textureNormal;
    }

    //private void PanicBreak()
    //{
    //    if (Input.GetButton("BrakeOrReverse"))
    //    {
    //        OnPanicBrake();
    //    }
    //    else
    //    {
    //        wheelFR.brakeTorque = 0;
    //        wheelFL.brakeTorque = 0;
    //        wheelRR.brakeTorque = 0;
    //        wheelRL.brakeTorque = 0;
    //    }
    //}

    public void OnHandBrake()
    {
        status.isBraking = true;
        status.isReverse = false;
        //wheelRL.motorTorque = 0;
        //wheelRR.motorTorque = 0;

        //wheelFR.brakeTorque = maxBrakeTorgue;
        //wheelFL.brakeTorque = maxBrakeTorgue;
        wheelRR.brakeTorque = maxBrakeTorgue;
        wheelRL.brakeTorque = maxBrakeTorgue;
        //sharedMaterialLights.mainTexture = textureBreaking;

        //TODO: Check if you hit something while you brake, if the car hits something its caput/driver is in shock
    }

    //protected void OnUpsideDownOrSide()
    //{
    //    status.SetAllBoolsToFalse();
    //    status.isInTrouble = true;
    //    //Stop engine or something more fun. Set smoke and put on fire
    //}

    #endregion



}
