using System;
using UnityEngine;

[System.Serializable]
public class PlayerController : BaseCar, IRacer, IEquatable<PlayerController>
{
    public IPlayerMovement movement;
    private IInputManager input;
    public IControlNobs controlNobs;
    public int playerId = -1; //do as readonly
    Vector3 recoveryPos;
    public FixedCamera fixedCamera;
    public Racer Racer { get; set; }
    private Equipment equipment;
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

    bool watchAIDrive;

    #region updates
    private void Start()
    {

        if (watchAIDrive)
        {

        }
        else
        {
            base.Init(false);
            isAIDriving = false;
            Debug.Assert(playerId != -1, "player input not set!");
            input = InputManager.Instance;
        }
    }

    void Update()
    {
        movement.Drivetrain.GearShifting(poweringWheels, playerId);
        movement.Drivetrain.CalculateRPM(poweringWheels, transform.InverseTransformDirection(rigid.velocity));
        movement.Steer();
        movement.BrakeOrReverse();
        movement.Accelerate(movement.Drivetrain.throttle);
        LookBehind();

        //Vector3 relativeVelocity = transform.InverseTransformDirection(rigdig.velocity);
        //drivetrain.GetCurrentSpeed(poweringWheels);

        UpdateInput(fixedCamera, equipment);
    }

    BaseCar something;
    void FixedUpdate()
    {
        controlNobs.RecoveryReset();
        controlNobs.CheckArmorStatus(); //TODO: stop the looping

        if (!raceStatus.gameOver)
        {
            //rigidBodySpeedKmH = rigid.velocity.magnitude * 3.6f; //speed to km/h;
            movement.Drivetrain.CalculateKmh(rigid);

            movement.Drivetrain.UpdateThrottleInput(); //this has to be in FixedUpdate or bad things happend!

            if (status.isEngineOn) //TODO: add this to drivetrain, so player can rev but cannot move
            {
                movement.Drivetrain.ApplyMotorTorque(poweringWheels, (int)TopSpeed, rigidBodySpeedKmH);

                controlNobs.TeleportToHighWay();
            }

            if (OnUpsideDown())
            {
                StartDeathCalculator();
            }
            else if (OnGroundSimpleVersion() == false && OnUpsideDown() == false) //No need to update everything on the air:
            {
                //print("Car is on air!");
                movement.AirMovement();
            }


            if (isDeathTimerOn == false) //temp hack for now
                lifeTime = 0;

            
            Racer.CollisionTracker.UpdateCollisionRacers();
            //if (ListenTargetCollisionObject(out something) == true)
            //{
            //    if (something.GetComponent<IRacer>() != null)
            //    {
            //        Debug.Log("CrashTimer started! Rae" + something.temporaryWorkAroundRacerId)
            //        Racer.CollisionTracker.StartCrashTimer(something.temporaryWorkAroundRacerId);
            //        something = null;
            //    }
            //}
        }
    }
    #endregion

    // Update is called once per frame
    public void UpdateInput(FixedCamera fixedCamera, Equipment equipment)
    {
        //bool cameraAngle = Input.GetButtonDown("CameraAngle");
        //bool UseOrFire = Input.GetButtonDown("UseOrFire");

        if (input.GetButtonDown(playerId, InputAction.CameraAngle))
        {
            fixedCamera.ChangeCameraView();
        }

        if (input.GetButtonDown(playerId, InputAction.UseOrFire))
        {
            equipment.UseTargetSelection(equipment.currentSelection);
        }

        if (input.GetButtonDown(playerId, InputAction.CycleAbilityNext))
        {
            equipment.CycleSelection(true);
        }

        if (input.GetButtonDown(playerId, InputAction.CycleAbilityPrevious))
        {
            equipment.CycleSelection(false);
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
        else if (input.GetButtonDown(playerId, InputAction.Mirror)) //button up
        {
            movement.LookForward();
        }
    }

    /// <summary>
    /// Creates a playable vehicle from any vehicle prefab 
    /// </summary>
    public void TransformAIRacerPrefabToPlayable(GameObject go, bool automaticTransmission, int playerId)
    {
        
        AIRacerController orginal = go.transform.GetComponent<AIRacerController>();
        this.playerId = playerId;

        //TODO: player = new PlayerController(playerNumber);
        this.isAIDriving = false;
        if (go.transform.GetComponent<AIRacerController>().textureNormal != null) //skipt these for now
        {
            this.textureNormal = orginal.textureNormal;
            this.textureBreaking = orginal.textureBreaking;
            this.materialLights = orginal.materialLights;
        }

        this.carCategory = orginal.carCategory; //probably useless
        CarSettings carSettings = CarData.GetCarSettings(orginal.carCategory, false); //AIRacers have different settings
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

        this.movement = new PlayerMovement(this, new Drivetrain(carSettings.motorTorqueMax, 7000, false, this.specs.realValues.numberOfGears));
        this.controlNobs = new ControlNobs(this);
        this.fixedCamera = new FixedCamera(this.style.prefabName);
        this.Armor = new Armor(carSettings.specs.armor);
        this.maxBrakeTorgue = carSettings.maxBrakeTorgue;

        if (carSettings.specs.vehicleName == "Formula F1 1989")
        {
            Camera.main.transform.parent = go.transform.GetChild(0).transform.Find("Body");
            this.RefreshRoadObject();
        }
        else     //TODO: remove this carbage when you have time
        {
            Camera.main.transform.parent = go.transform;
            this.RefreshRoadObject();
        }

        this.rigid.mass = carSettings.specs.realValues.mass;
        this.rigid.centerOfMass = new Vector3(0, -1.5f, 0);

        Camera.main.farClipPlane = VideoSettings.CameraFarPlane;
        this.fixedCamera.ChangeCameraView(CameraView.ThirdPerson); //ThirdPerson is the default
        go.name = "Racer " + (playerId + 1) + "(Player)";
        this.Racer = new Racer(playerId, "Lakupekka", "FU-P0L1C3", false);
        this.equipment = go.AddComponent<Equipment>();
        this.equipment.Init(this.specs.weaponSlots);

        //and lastly remove useless stuff:
        orginal.enabled = false;
        GameObject.Destroy(orginal); //Removes the RacerController, we dont need it anymore
        go.SetActive(false); //nessaccary?
        go.SetActive(true); //... heck, lets do it anyway
        go.GetComponent<BaseCar>().status.isEngineOn = false; //Player cannot move before the go-launch

        Debug.Assert(this.maxBrakeTorgue != 0, "maxBrakeTorgue  0 !!");

    }

    #region overrideEquals
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

        //if (p.status.isEngineOn != status.isEngineOn) //Motor is on for now
        //{
        //    return false;
        //}

        return true;
    }

    #endregion
}