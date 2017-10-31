
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
    public CarCategory carCategory; //TODO:put this to VehicleStyle class when you have time
    public VehicleStyle style;

    [Header("Set by programmatically")]
    [ReadOnly] public CarSpecs specs;

    public Sensors sensors;
    public RoadNode currentRoadNode = new RoadNode();
    [HideInInspector] public PathController pathController;
    [Header("Visuals")]
    public Texture2D textureNormal;
    public Texture2D textureBreaking;
    public Renderer carRenderer;
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
    private float rigidBodySpeedKmHForEngine; //rigdig.velocity.magnitude * 3.6f
    public float RigidBodySpeedKmHForEngine
    {
        get { return rigidBodySpeedKmHForEngine; }
        set { rigidBodySpeedKmHForEngine = value; }
    }

    private float rigidBodySpeedKmH;
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
    [HideInInspector]
    public float simpleDistToGround;

    //  public Rigidbody rigdig;
    [HideInInspector] public float yawSpeed;

    //precalculated values;
    public float wheelSpinningWithoutRpm;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public WheelType wheelType;
    public WheelCollider[] poweringWheels;

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

    public void CalculateRigidBodySpeed()
    {
        rigidBodySpeedKmHForEngine = rigid.velocity.magnitude * 3.6f;
    }

    public void Init(bool isAIDriving, InitType type)
    {     
        if (carCategory == CarCategory.NotSet)
        {
            throw new Exception("carCategory cannot be 'Notset' and must be set explicitly from Inspector for each vehicle!");
        }

        CarSettings carSettings = CarData.GetCarSettings(carCategory, isAIDriving);
        CarData.PopulateCarSettings(gameObject, carSettings, type);
        SetUniveralsSettings(carSettings);
        PreCalculateFunctions();
    }

    public void SetUniveralsSettings(CarSettings carSettings)
    {
        SetCarWheelType(carSettings.wheelType);
        sensors = new Sensors(carCategory);
    }

    protected void SetCarWheelType(WheelType t)
    {
        wheelType = t;
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
        if (raceStatus.isStartingAtStartingLine) //TODO erase this check
        {
            print("Its game over for racer " + gameObject.name);
        }
        status.isEngineOn = false;
        raceStatus.gameOver = true;

        StartCoroutine(DisableCalculatorAfter(5));
    }

    protected void PreCalculateFunctions()
    {
        wheelSpinningWithoutRpm = 2 * Mathf.PI * wheelFL.radius * 60 / 1000;
    }
		
    public void SetSteeringAngle_SteeringWheel(float targetAngle)
    {
        wheelFL.steerAngle = targetAngle;
        wheelFR.steerAngle = targetAngle;
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

        foreach (WheelCollider w in poweringWheels)
        {
            w.brakeTorque = brakingForce;
        }
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
        status.handBrake = false;
        BrakeWithAllTires(0);
    }

    public void OnHandBrake()
    {
        status.handBrake = true;
        status.isBraking = true;
        status.isReverse = false;
        wheelRR.brakeTorque = maxBrakeTorgue;
        wheelRL.brakeTorque = maxBrakeTorgue;
        //sharedMaterialLights.mainTexture = textureBreaking;

        //TODO: Check if you hit something while you brake, if the car hits something its caput/driver is in shock
    }


    #endregion



}
