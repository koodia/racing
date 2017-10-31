using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public sealed class PlayerController : BaseCar, IRacer, IEquatable<PlayerController>, IControlDrift
{
    public IPlayerMovement movement;
    private IInputManager input;
    public IControlNobs controlNobs;
    public int playerId = -1;
    public FixedCamera fixedCamera;
    public Racer Racer { get; set; }
    public Equipment equipment;
    public Equipment Equipment
    {
        get
        {
            return equipment;
        }
        set
        {
            equipment = value;
        }
    }

    bool watchAIDrive = false;

    //ControlDrift:
    public bool RapidWheelStepOne { get; set; }
    public bool RapidWheelStepTwo { get; set; }
    public bool RapidWheelStepThree { get; set; }
    public float ControlDriftTimer { get; set; }
    bool checkingTurn;

    #region updates
    protected override void Start()
    {
        base.Start();

        if (watchAIDrive)
        {
            //isAIDriving = true;
            //base.Init(isAIDriving, InitType.Racer);
        }
        else
        {
            carCategory = gameObject.GetComponent<BaseCar>().carCategory;
            if (carCategory == CarCategory.NotSet)
            {
                throw new Exception("carCategory cannot be 'Notset' and must be set explicitly from Inspector!");
            }

            //visuals.materialLights = visuals.carRenderer.materials[2]; //TODO
            isAIDriving = false;
            CarSettings carSettings = CarData.GetCarSettings(carCategory, isAIDriving);
            CarData.PopulateCarSettings(gameObject, carSettings, InitType.Racer);
            RefreshRigidbody(gameObject.GetComponent<BaseCar>().style.prefabName);
            SetCarWheelType(carSettings.wheelType);
            sensors = new Sensors(carCategory);
            PreCalculateFunctions();

            Debug.Assert(playerId != -1, "player input not set!");
            StartCoroutine(CheckRapidTurns(1f));
        }
    }

    new void Update()
    {
        DebugStuff.DebugUpdate(Armor);
        DebugStuff.StopTorgue(status, justKeeptStillForDebugging);

        movement.Drivetrain.GearShifting(poweringWheels, playerId);
        movement.Drivetrain.CalculateRPM(poweringWheels, transform.InverseTransformDirection(rigid.velocity));
        RigidBodySpeedKmH = movement.Drivetrain.SpeedRigidKmh; ////for StopZone


        if (input.UseSteeringWheel)
        {
            movement.Steer();
            movement.BrakeOrReverse();
            movement.Throttle();
            movement.Clutch();
        }
        else
        {
            movement.SteerByKeyboard();
            movement.BrakeOrReverseByKeyboard();
            movement.ThrottleByKeyboard();
        }
        movement.Accelerate(movement.Drivetrain.throttle);
        LookBehind();

        UpdateInput(fixedCamera, equipment);

    }

    void FixedUpdate()
    {
        controlNobs.RecoveryReset();
        controlNobs.CheckArmorStatus(); //Why are you looping this... Fix

        if (!raceStatus.gameOver)
        {
            movement.Drivetrain.CalculateKmh(rigid);

            if (status.isEngineOn) //TODO: add this to drivetrain, so player can rev but cannot move
            {
                movement.Drivetrain.ApplyMotorTorque(poweringWheels, (int)TopSpeed);

                controlNobs.TeleportToHighWay();
            }

            if (OnUpsideDown())
            {
                StartDeathCalculator();
            }
            else if (OnGroundSimpleVersion() == false && OnUpsideDown() == false) //No need to update everything on the air:
            {
                print("Car is on air!");

                if (UnityInputManager.Instance.UseSteeringWheel)
                {
                    movement.AirMovement();
                }
                else
                {
                    movement.AirMovementByKeyboard();
                }
            }


            if (isDeathTimerOn == false) //temp hack for now
                lifeTime = 0;


            Racer.CollisionTracker.UpdateCollisionRacers();

            OnControlDrifting();
        }
    }
    #endregion




    // Update is called once per frame
    public void UpdateInput(FixedCamera fixedCamera, Equipment equipment)
    {

        if (input.GetButtonDown(playerId, InputAction.CameraAngle))
        {
            fixedCamera.ChangeCameraView();
        }

        if (input.GetButtonDown(playerId, InputAction.UseOrFire))
        {
            equipment.UseTargetSelection(equipment.currentSelection);
        }

        if (UnityInputManager.Instance.UseSteeringWheel)
        {

            if (input.GetArrowKey(9000))
            {
                equipment.CycleSelection(true);
            }

            if (input.GetArrowKey(27000))
            {
                equipment.CycleSelection(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.A)) //godmode 
        {
            equipment.ActivateAllAbilities();
            Debug.Log("All abilities unlocked with ammo");
        }

        if (Input.GetButtonDown("Nitro"))
        {
            equipment.UseNitro();
        }

    }

    private void LookBehind()
    {
        if (input.GetButtonDown(playerId, InputAction.Mirror)) //held
        {
            movement.LookBack();
        }
        else if (input.GetButtonUp(playerId, InputAction.Mirror))
        {
            movement.LookForward();
        }
    }


    public void OnControlDrifting()
    {
        if (status.controlDrifting)
        { 
            //TODO: dont rotate CameraX

            Debug.Log("YOU ARE CONTROL DRIFTING!");

            if (Input.GetButton("BrakeOrReverse") || RigidBodySpeedKmH < 60)
            {
                status.controlDrifting = false;
                Debug.Log("Control Drifting ended");
            }
        }
        //else
        //{
        //    status.controlDrifting = false;
        //   // Debug.Log("Control Drifting ended");
        //}
    }

    public IEnumerator BrakeAndEndRace()
    {
       
        status.isEngineOn = false;
        OnBrake();
        movement.Drivetrain.KillPower(); //stops power


        wheelFR.brakeTorque = 200000;
        wheelFL.brakeTorque = 200000;
        wheelRR.brakeTorque = 200000;
        wheelRL.brakeTorque = 200000;
      
        yield return new WaitUntil(() => RigidBodySpeedKmH < 0.3);
    }


    int nextChassyRotationClockwise = 1;
    public bool CalculateChassisAngle(float degrees, Vector3 capturedPos)
    {
        var targetPoss = Quaternion.AngleAxis(degrees, Vector3.up) * capturedPos;
        float angleRad = Mathf.Atan2(gameObject.transform.forward.y - targetPoss.y, gameObject.transform.forward.x - targetPoss.x);
        float angleDeg = (180 / Mathf.PI) * angleRad;

        if (Math.Abs(angleDeg) > Math.Abs(degrees))
        {
            nextChassyRotationClockwise = (nextChassyRotationClockwise > 0) ? 1 : -1;
            return true;
        }
        return false;
    }

    public bool CalculateChassisAngleFirst(float degrees, Vector3 capturedPos)
    {
        // Get Angle in Radians
        float angleRad = Mathf.Atan2(gameObject.transform.forward.x - capturedPos.x, gameObject.transform.forward.z - capturedPos.z);
        // Get Angle in Degrees
        float angleDeg = (180 / Mathf.PI) * angleRad;
       // Debug.Log("Wihtout abs:" + angleDeg);

        if (Math.Abs(angleDeg) > 90)
        {
            nextChassyRotationClockwise = (angleDeg > 0) ? 1 : -1;
            return true;
        }
        return false;
    }

    //public bool CalculateChassisAngleHundredDegrees(float degrees, int clockwise = 0)
    //{
    //    Vector3 capturedPos = gameObject.transform.forward;

    //    // Get Angle in Radians
    //    float AngleRad = Mathf.Atan2(gameObject.transform.forward.y - Vector3.forward.y, gameObject.transform.forward.x - Vector3.forward.y);
    //    // Get Angle in Degrees
    //    float AngleDeg = (180 / Mathf.PI) * AngleRad;
    //}

    //public bool CalculateChassisAngle(float degrees, bool clockwise)
    //{
    //    // Vector2 diference = Vector2.zero;

    //    // the vector that we want to measure an angle from
    //    Vector3 referenceForward = gameObject.transform.forward; /* some vector that is not Vector3.up */
    //                               // the vector perpendicular to referenceForward (90 degrees clockwise)
    //                               // (used to determine if angle is positive or negative)
    //    Vector3 referenceRight = Vector3.Cross(Vector3.up, referenceForward);

    //    Vector3 newDirection =   Quaternion.Euler(0, 100 * nextChassyRotationClockwise, 0) * gameObject.transform.forward;

    //    // Get the angle in degrees between 0 and 180
    //    float angle = Vector3.Angle(newDirection, referenceForward);
    //    // Determine if the degree value should be negative.  Here, a positive value
    //    // from the dot product means that our vector is on the right of the reference vector   
    //    // whereas a negative value means we're on the left.
    //    float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
    //    float resultAngle = sign * angle;

    //    if (resultAngle > degrees)
    //    {
    //        nextChassyRotationClockwise = true;
    //        return true;
    //    }

    //    return false;
    //}

    //Very short timeframe to perform
    public IEnumerator CheckRapidWheelStepOne(float timeFrameToPerform, Vector3 capturedforwardPos)
    {
        var clock = timeFrameToPerform;
        
        while (clock > 0)
        {
            clock -= Time.deltaTime;
            if (CalculateChassisAngleFirst(40f, capturedforwardPos))
            {
                Debug.Log("First check passed");
               var nextforwardTarget = gameObject.transform.forward;
                StartCoroutine(CheckRapidWheelStepTwo(0.5f, nextforwardTarget));
                clock = 0; //"break"
            }

            yield return null;
        }

        RapidWheelStepTwo = false;
    }

    //Keeps checking forever
    public IEnumerator CheckHandBrakePressed(float timeFrameToPerform)
    {
        var clock = timeFrameToPerform;
        while (clock > 0)
        {
            clock -= Time.deltaTime;

            if (status.handBrake)
            {
                status.handBrakePressed = true;
                clock = 0; //"break"
            }

            yield return null;
        }
        status.handBrakePressed = false;
    }

    //Keeps checking forever
    public IEnumerator CheckRapidWheelStepTwo(float timeFrameToPerform, Vector3 forwardTargetPos)
    {
        var clock = timeFrameToPerform;
        while (clock > 0)
        {
            if (CalculateChassisAngle(100f, forwardTargetPos) && status.handBrakePressed)
            {
                Debug.Log("Second check passed");
                var nextforwardTarget = gameObject.transform.forward;
                StartCoroutine(CheckRapidWheelStepThree(0.5f, nextforwardTarget));
                clock = 0; //"break"
            }

            yield return null;
        }
        RapidWheelStepTwo = false;
    }

    //Keeps checking forever
    public IEnumerator CheckRapidWheelStepThree(float timeFrameToPerform, Vector3 forwardTargetPos)
    {
        var clock = timeFrameToPerform;
        while (clock > 0)
        {
            clock -= Time.deltaTime;

            if (CalculateChassisAngle(80f, forwardTargetPos))
            {
                Debug.Log("Third check passed");
                status.controlDrifting = true; //now start drifting!
                RapidWheelStepOne = false;
                RapidWheelStepTwo = false;
                RapidWheelStepThree = false;
                clock = 0; //"break"
            }

            yield return null;
        }

        RapidWheelStepOne = false;
        RapidWheelStepTwo = false;
        RapidWheelStepThree = false;

    }

    //Keeps checking forever
    public IEnumerator CheckRapidTurns(float timeFrameToPefrormFirst90DegreeTurn)
    {
        while (true)
        {
            if (!checkingTurn)
            {
               Vector3 capturedPos = gameObject.transform.forward;
               StartCoroutine(CheckRapidWheelStepOne(timeFrameToPefrormFirst90DegreeTurn, capturedPos));
               checkingTurn = true;

                yield return new WaitForSeconds(timeFrameToPefrormFirst90DegreeTurn);
                RapidWheelStepOne = false;
                RapidWheelStepTwo = false;
                RapidWheelStepThree = false;
                checkingTurn = false;
            }

            yield return null;
        }
    }


    #region Initialization

    /// <summary>
    /// Creates a playable vehicle from any vehicle prefab 
    /// </summary>
    public void TransformAIRacerPrefabToPlayable(GameObject go, bool automaticTransmission, int playerId)
    {
        AIRacerController orginal = go.transform.GetComponent<AIRacerController>();
        this.playerId = playerId;
        this.isAIDriving = false;
        if (go.transform.GetComponent<AIRacerController>().textureNormal != null) //skipt these for now
        {
            this.textureNormal = orginal.textureNormal;
            this.textureBreaking = orginal.textureBreaking;
            this.materialLights = orginal.materialLights;
        }

        this.carCategory = orginal.carCategory; //probably useless
        CarSettings carSettings = CarData.GetCarSettings(orginal.carCategory, isAIDriving); //AIRacers have different settings
        PlayerController player = go.transform.GetComponent<PlayerController>();
        RefreshRigidbody(orginal.style.prefabName);
        player.rigid.mass = carSettings.specs.realValues.mass;
        player.rigid.centerOfMass = new Vector3(0, -1.5f, 0);

        //TODO: remove this carbage when you fix your car
        if (carSettings.specs.vehicleName == "Formula F1 1989")
        {
            Camera.main.transform.parent = go.transform.GetChild(0).transform.Find("Body");
        }
        else
        {
            Camera.main.transform.parent = go.transform;
        }

        this.specs = carSettings.specs;
        this.style = orginal.style;
        this.wheelFL = orginal.wheelFL;
        this.wheelFR = orginal.wheelFR;
        this.wheelRL = orginal.wheelRL;
        this.wheelRR = orginal.wheelRR;
        this.wheelType = orginal.wheelType;
        this.poweringWheels = orginal.poweringWheels;
        this.carRenderer = orginal.carRenderer;
        this.visualsObjectInHierarchy = orginal.visualsObjectInHierarchy;

        this.movement = new PlayerMovement(this, new Drivetrain(carSettings.specs.realValues.torgue, carSettings.specs.realValues.maxRpm, false, this.specs.realValues.numberOfGears));

        Transform point = GameObject.Find("FastTravelPoint").transform; //for debug usage only
        Debug.Assert(point != null, "could not find FastTravelPoint");
        this.controlNobs = new ControlNobs(this, point.localPosition);
        this.fixedCamera = new FixedCamera(this.style.prefabName);
        this.Armor = new Armor(carSettings.specs.realValues.armor);
        this.maxBrakeTorgue = carSettings.specs.realValues.braking;
        this.turnSpeed = carSettings.specs.realValues.turnSpeed;
        this.maxSteerAngle = carSettings.specs.realValues.maxSteerAngle;
        this.simpleDistToGround = carSettings.simpleDistToGround;
        this.yawSpeed = carSettings.yawSpeed;

        Camera.main.farClipPlane = VideoSettings.CameraFarPlane;
        this.fixedCamera.ChangeCameraView(CameraView.ThirdPerson); //ThirdPerson is the default
        go.name = "Racer " + (playerId + 1) + "(Player)";
        this.Racer = new Racer(playerId, "Lakupekka", "FU-P0L1C3", false);
        this.equipment = go.AddComponent<Equipment>();
        this.equipment.Init(this.specs.weaponSlots);
        this.cargo = orginal.cargo;

        //and lastly remove useless stuff:
        orginal.enabled = false;
        Destroy(orginal); //Removes the RacerController, we dont need it anymore
        go.SetActive(false); //nessaccary?
        go.SetActive(true); //... heck, lets do it anyway
        go.GetComponent<BaseCar>().status.isEngineOn = false; //Player cannot move before the go-launch
        input = InputManager.Instance;
     
        Debug.Assert(input != null, "input is null");
    }

    #endregion

    #region overrideEquals
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
    public override bool Equals(System.Object obj)
    {
        var other = obj as PlayerController;
        if (other == null) return false;

        return Equals(other);
    }
    public bool Equals(PlayerController obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        PlayerController p = obj as PlayerController;
        if ((System.Object)p == null)
        {
            return false;
        }

        if (p.isAIDriving != isAIDriving)
        {
            return false;
        }

        if (p.carCategory != carCategory)
        {
            return false;
        }

        if (p.style.Equals(style))
        {
            return false;
        }

        if (p.wheelFL == null || p.wheelFR == null || p.wheelRL == null || p.wheelRR == null)
        {
            return false;
        }

        if (p.wheelType != wheelType)
        {
            return false;
        }

        if (p.poweringWheels.Length != poweringWheels.Length)
        {
            return false;
        }

        if (p.carRenderer == null)
        {
            return false;
        }

        if (p.specs.Equals(specs))
        {
            return false;
        }

        //These would be enough
        if (p.specs.vehicleName != specs.vehicleName &&
            p.specs.specTopSpeed != specs.specTopSpeed &&
            p.specs.specMass != specs.specMass &&
            p.specs.specTorgue != specs.specTorgue &&
            p.specs.carCategory != specs.carCategory &&
            p.specs.description != specs.description)
        {
            return false;
        }

        if (
            p.specs.realValues.topSpeed != specs.realValues.topSpeed &&
            p.specs.realValues.mass != specs.realValues.mass &&
            p.specs.realValues.torgue != specs.realValues.torgue &&
            p.specs.realValues.turnSpeed != specs.realValues.turnSpeed)

        {
            return false;
        }

        if (p.movement == null) 
        {
            return false;
        }

        if (p.movement.Drivetrain == null)
        {
            return false;
        }

        if (p.controlNobs == null)
        {
            return false;
        }

        if (p.fixedCamera == null)
        {
            return false;
        }

        if (p.fixedCamera.cameraView != CameraView.ThirdPerson)
        {
            return false;
        }

            if (p.Armor == null)
        {
            return false;
        }

        if (p.maxBrakeTorgue != maxBrakeTorgue)
        {
            return false;
        }

        if (p.maxBrakeTorgue == 0)
        {
            return false;
        }

        if (p.style.prefabName != style.prefabName)
        {
            return false;
        }

        if (Camera.main.farClipPlane == 0)
        {
            return false;
        }

        if (p.name != "Racer 1(Player)")
        {
            return false;
        }

        if (p.equipment == null)
        {
            return false;
        }

        if (p.equipment.abilities.Length == 0)
        {
            return false;
        }

        if (p.equipment.abilities.Length != equipment.abilities.Length)
        {
            return false;
        }

        if (null != p.GetComponent<TrafficVehicleController>()) //There should not be a TrafficController anymore
        {
            return false;
        }

        if (null != p.GetComponent<AIRacerController>()) //There definetly should not be a AIRacerController anymore
        {
            return false;
        }

        return true;
    }

    public void ControlDrifting()
    {
        throw new NotImplementedException();
    }

    #endregion
}