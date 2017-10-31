using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIBaseCar : BaseCar, IEmulateDriving
{
    [HideInInspector] public float visionRange = VideoSettings.CameraFarPlane; //VisionRange vary depending on car y position. So in bridge you will see further
    public bool isVisionRange { get; set; }
    public int currentNodeIndex;
    [HideInInspector] public Transform currentPath;
    [ReadOnly] public bool isChangingLane;
    [HideInInspector] private const int maximumNodeJumpWhenSwitchingLane = 3;
    [HideInInspector] private int previousNodeCount = 0;
    [HideInInspector] public const float laneSwitchTimeChangeTime = 14; //depending on the traffic density. Rarely in heavy traffic
    [HideInInspector] public int previousNodeIndex = 0;

    //For stuck testing:
    [ReadOnly] public bool fixingStuck; //temp variable
    private bool takeNewMeasure;
    Vector3 previousStuckPosition;
    private const float brakingFactor = 1; //The higher the value the sooner the car will brake in speeds under 20
    //AI lane switch
    protected float nodeSwitchChangePeriodInSeconds = 0;

    protected override void Awake()
    {
        currentNodeIndex = 0;
        //Init to unrational values
        currentRoadNode.nodeNumber = -1;
        currentRoadNode.laneIndex = -1;
        previousNodeIndex = -1;

        pathController = gameObject.GetComponent<PathController>();
        if (pathController == null)
        {
            throw new NullReferenceException("could not find pathController attached to gameObject");
        }
    }

    protected override void Start()
    {

    }

    #region Emulation

    float rigidMagnitudeWhileEmulateTransition;
    bool firstPushGiven;
    //There is more elegant solution if you add one more level in the hierarchy
    protected void DisableEverythingExceptController(bool disable)
    {
        if (disable) //emulating
        {
            rigidMagnitudeWhileEmulateTransition = rigid.velocity.magnitude;
            rigid.isKinematic = disable; //Any side effects? How is the ridgid physics behaving!?
            pathController.enabled = !disable;
            visualsObjectInHierarchy.SetActive(!disable);
            firstPushGiven = false;
            
        }
        else //back to vision
        {
            if (!firstPushGiven)
            {
                pathController.enabled = !disable;
                visualsObjectInHierarchy.SetActive(!disable);
                rigid.isKinematic = disable;
                rigid.velocity = new Vector3(0, 0, tcurSpeed);
                Debug.Log("back with speed:" + rigid.velocity.magnitude);
                firstPushGiven = true;
                setToMotorwayHeightAlready = false;
            }
        }
    }

    public void EmulateLaneSwitch()
    {

    }

    protected float tcurSpeed;
    protected float[] emulatedCarList = new float[200];
    protected bool initialSpeed = false;
    bool setToMotorwayHeightAlready;
    public void EmulateDriveForward()
    {
        if (!setToMotorwayHeightAlready)
        {
            transform.position.Set(transform.position.x, 0.5f, transform.position.z);  //TODO: lower Y motorway height:
            setToMotorwayHeightAlready = true;
        }

        if (tcurSpeed < TopSpeed) //28kbs = 100.8 kmh
        {
            if (!initialSpeed)
            {
                tcurSpeed = rigidMagnitudeWhileEmulateTransition;
                initialSpeed = true;
            }
            else
            {
                tcurSpeed += accelerationMetersPerSec / 3.6f; //100km/h by 3.6;
            }
        }
        else
        {
            tcurSpeed = TopSpeed;
        }
        CurrentSpeed = tcurSpeed;

        Vector3 drivingLane = new Vector3(pathController.lanes[pathController.currentLaneIndex].transform.position.x, transform.position.y, transform.position.z);
        transform.SetPositionAndRotation(drivingLane, Quaternion.identity);
        transform.Translate(0, 0, tcurSpeed * Time.deltaTime);

    }

    public void EmulateLetAIDrive()
    {
        if (!raceStatus.roadEnds)
        {
            EmulateDriveForward();
            //TODO: EmulateLaneSwitch();
            //TODO: EmulateNesacceryLaneSwitch();
            CheckWayPointDistance(pathController.lanes[pathController.currentLaneIndex].GetComponent<Path>().roadDirectionOpposite);
        }
    }


    #endregion

    #region SituationSolvin


    IEnumerator BrakeAndEndRace()
    {
        status.isEngineOn = false;
        OnBrake();
        yield return new WaitUntil(() => RigidBodySpeedKmH < 0.3);
    }

    IEnumerator IEnumeratorGoAround()
    {
        OnThrottle();
        Throttle(true);
        //Enable side sensors for short moment after reversing
        currentRoadNode.disableRacersSideSensors = false;
        yield return new WaitForSeconds(5); //.. so hopefully we can go around inside 7 seconds

        if (raceStatus.isStartingAtStartingLine)
        { currentRoadNode.disableRacersSideSensors = true; } //traffic cars have usually disabled all times. This line has not been tested

        nodeSwitchChangePeriodInSeconds = UnityEngine.Random.Range(5, 7);
    }

    public IEnumerator FixStuck()
    {
        if (status.stuckCounter < 2)
        {
            fixingStuck = true;
            yield return StartCoroutine(OnReverse(2, true));

            fixingStuck = false;
        }
        else if (status.stuckCounter > 3)
        {
            status.isInTrouble = true;
            GameOver();
        }
        else
        {
            fixingStuck = true;
            yield return StartCoroutine(OnReverse(3, false)); //Lets try reversing 4 seconds and with tyres straight
            fixingStuck = false;
        }
        status.stuckCounter++;
    }

    public void CheckStuck()
    {
        if (!fixingStuck)
        {
            nodeSwitchChangePeriodInSeconds -= Time.deltaTime;
            if (takeNewMeasure)
            {
                previousStuckPosition = transform.position;
                takeNewMeasure = false;
            }

            if (nodeSwitchChangePeriodInSeconds < 0)
            {
                if (currentRoadNode.nodeNumber == previousNodeIndex && RigidBodySpeedKmH < 20) //We can do one more checking before vector math
                {
                    //Check if the car has not moved enough
                    if (Vector3.Distance(previousStuckPosition, transform.position) < 50f)
                    {
                        status.isStuck = true;
                    }
                }

                takeNewMeasure = true;
                nodeSwitchChangePeriodInSeconds = UnityEngine.Random.Range(5, 7); //humanize the reaction time a bit
            }
        }
    }


    #endregion

    #region Navigation

    protected virtual void SpontaniousLaneSwitch()
    {

    }

    /// <summary>
    /// There is a bug here in the design. When this is called first time, the .GetRoadNodes() will return null;
    /// </summary>
    protected virtual void CheckStartPositionAndNode()
    {
        currentNodeIndex = pathController.GetClosestObjectNonSpecialRoadNode(pathController.lanes[pathController.currentLaneIndex].GetComponent<Lane>().GetRoadNodes(), gameObject.transform);  //TODO: fix the bug
        currentNodeIndex++; //pick node from further so vehicle does not spin around
        currentRoadNode = pathController.lanes[pathController.currentLaneIndex].GetComponent<Lane>().GetRoadNodes()[currentNodeIndex];
    }



    //   /// <summary>
    //   ///  So at the road level:  Camera.main.transform.position.y  = 0  THEN  visionRange = 1200
    //   ///  and flying like a bird: Camera.main.transform.position.y  = 140  THEN  visionRange = ~2500
    //   ///  There is a features that if the player jumps quickly up in the air, the visionRange might not response in time. Change visualRangeTimer value for quicker response
    //   /// </summary>
    //   /// 
    public void CheckVisualRange()
    {
        if (Camera.main == null)
        { return; }

        visionRange = VideoSettings.CameraFarPlane + Camera.main.transform.position.y * 10f;
        if (Vector3.Distance(Camera.main.transform.position, transform.position) > visionRange)
        {
            isVisionRange = false;
        }
        else
        {
            isVisionRange = true;
        }
    }


    public virtual float GetLaneSpeed(int laneIndex)
    {
        if (currentRoadNode.speed == 0)
        {
            switch (laneIndex)
            {
                //Slower lanes
                case 0:
                case 1:
                case 6:
                case 7:
                    return 50f;
                case 2:
                case 3:
                case 4:
                case 5:
                    return 60f;
                default: throw new Exception("No suck lane index!:" + laneIndex);
            }
        }
        else
        {
            if (raceStatus.isStartingAtStartingLine)
            {
                return VideoSettings.MaxSpeed; //full on!
            }
            else
            {
                return currentRoadNode.speed;
            }
        }
    }

    public void CheckWayPointDistance(bool laneDirectionOpposite)
    {
        if (laneDirectionOpposite)
        {
            // CheckWayPointDistanceOpposite(); //TODO: in future
        }
        else
        {
            CheckWayPointDistanceForward();
        }
    }

    public virtual void OnMotorwayEnd()
    {

    }

    //TODO: Rewrite/make more dynamic version
    protected void CheckWayPointDistanceForward()
    {

        var targetLane = pathController.lanes[pathController.currentLaneIndex].GetComponent<Lane>();
        if (currentRoadNode.sectionType != SectionType.LaneGoesAlongX)
        {
            if (Vector3.Distance(transform.position, targetLane.GetRoadNodes()[currentRoadNode.nodeNumber].pos) < distanceToSwitchNode
                || targetLane.GetRoadNodes()[currentRoadNode.nodeNumber].pos.x > transform.position.z) //Check also if the car has passed the node already
            {
                if (currentRoadNode.nodeNumber >= targetLane.GetRoadNodes().Length - 1)
                {
                    OnMotorwayEnd();
                }
                else
                {
                    currentNodeIndex++;
                    UpdateNodeInfo(targetLane);
                }
            }
        }
        else /// SectionType is LaneGoesAlongX. We also have to check the Y on the bridge
		{
            if (Vector3.Distance(transform.position, targetLane.GetRoadNodes()[currentRoadNode.nodeNumber].pos) < distanceToSwitchNode
                || targetLane.GetRoadNodes()[currentRoadNode.nodeNumber].pos.z < transform.position.x  //if the car has passed the node already. See the x!
                && targetLane.GetRoadNodes()[currentRoadNode.nodeNumber].pos.y - 20 < transform.position.y) // Height as well ,so at least the node height minus some height because the node bight be higher than the car
            {
                {
                    if (currentRoadNode.nodeNumber == targetLane.GetRoadNodes().Length - 1)
                    {
                        OnMotorwayEnd();
                    }
                    else
                    {
                        currentNodeIndex++;
                        UpdateNodeInfo(targetLane);
                    }
                }
            }
        }
    }



    //TODO not ready:
    protected void UpdateNodeInfo(Lane lane)
    {
        if (currentNodeIndex < lane.GetRoadNodes().Length)
        {
            status.stuckCounter = 0; //jippii car advanced to the next roadNode
            previousNodeIndex = currentNodeIndex; //lane.GetRoadNodes()[currentNodeIndex].nodeNumber;
            currentRoadNode = lane.GetRoadNodes()[currentNodeIndex];

            //TODO: remove this quick hax
            if (raceStatus.isStartingAtStartingLine == false) //configuration for traffic cars
            {
                currentRoadNode.disableRacersSideSensors = true;
            }
        }
    }

    public void SwitchLane2(Transform newLane)
    {
        gameObject.GetComponent<AIRacerController>().currentPath = newLane;

        if (previousNodeCount + 3 < currentNodeIndex + maximumNodeJumpWhenSwitchingLane)
        {
            currentNodeIndex += 3;//Skip some nodes so the transition will be smoother
            previousNodeCount = currentNodeIndex;
        }
        else if (pathController.lanes[pathController.currentLaneIndex].GetComponent<Lane>().GetRoadNodes().Length <= currentNodeIndex) //JOULUPUKKI
        {
            status.isPanicBraking = true; //just stop for fun
        }
        else
        {
            //just switch lane but dont jump any nodes if there is plenty of nades between
        }
    }

    public void SwitchLane(Transform newLane)
    {
        currentPath = newLane;

        if (previousNodeCount + 3 < currentNodeIndex + maximumNodeJumpWhenSwitchingLane)
        {
            currentNodeIndex += 3;//Skip some nodes so the transition will be smoother
            previousNodeCount = currentNodeIndex;
        }
        else if (pathController.lanes[pathController.currentLaneIndex].GetComponent<Lane>().GetRoadNodes().Length <= currentNodeIndex) //JOULUPUKKI
        {
            status.isPanicBraking = true; //just stop for fun
        }
        else
        {
            //just switch lane but dont jump any nodes if there is plenty of nades between
        }
    }

    public virtual void Sensors()
    {

    }

    /// <summary>
    /// Raycasting depends on speed so there enough time to react
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    protected float FrontSensorLengthBySpeed(SensorPlacement sensor, float speed)
    {
        switch (sensor)
        {
            case SensorPlacement.FrontMiddle:
                if (speed < 20)
                {
                    return 30; //keep some distance
                }
                else
                {
                    return speed * brakingFactor;
                }

            case SensorPlacement.FrontSide:
                if (speed < 20)
                {
                    return 30;
                }
                else
                {
                    return speed * brakingFactor * 0.7f; //30% shorter
                }

            case SensorPlacement.Angle:
                return 15;
            default:
                throw new NotImplementedException("not yet for sensor:" + sensor);
        }
    }


    #endregion

    #region Movement


    protected void Throttle2(bool forwardDir)
    {
        if (CurrentSpeed < TopSpeed)
        {
            if (!status.isBraking)
            {
                if (isChangingLane || status.keepBreaking) //No throttle when turning
                {
                    for (int i = 0; i < poweringWheels.Length; i++)
                    {
                        poweringWheels[i].motorTorque = 0;
                    }
                }
                else
                {
                    for (int i = 0; i < poweringWheels.Length; i++)
                    {
                        poweringWheels[i].motorTorque = motorTorqueMax * Convert.ToInt32(forwardDir);
                    }

                    if (status.isReverse)
                    {
                        status.isThrottle = false;
                    }
                    else
                    { status.isThrottle = true; }
                }
            }
        }
        else
        {
            for (int i = 0; i < poweringWheels.Length; i++)
            {
                poweringWheels[i].motorTorque = 0;
            }

            //wheelRL.motorTorque = 0;
            //wheelRR.motorTorque = 0;
        }
    }

    protected void Throttle(bool forwardDir)
    {
        if (CurrentSpeed < TopSpeed)
        {
            if (!status.isSpeedLimiterOn)
            {
                if (!status.isBraking)
                {
                    if (isChangingLane || status.keepBreaking) //No throttle when turning
                    {
                        for (int i = 0; i < poweringWheels.Length; i++)
                        {
                            poweringWheels[i].motorTorque = 0;
                        }
                    }
                    else //esim pakitetaan stuckissa
                    {
                        for (int i = 0; i < poweringWheels.Length; i++)
                        {
                            poweringWheels[i].motorTorque = motorTorqueMax * Convert.ToInt32(forwardDir);
                        }

                        if (status.isReverse)
                        {
                            status.isThrottle = false;
                        }
                        else
                        { status.isThrottle = true; }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < poweringWheels.Length; i++)
            {
                poweringWheels[i].motorTorque = 0;
            }
        }
    }

    IEnumerator ReleaseThrottleForSeconds(float seconds)
    {
        isChangingLane = true;
        status.isThrottle = false;
        //TODO:  turning should also be more subtle

        yield return new WaitForSeconds(seconds);
        isChangingLane = false;
    }

    protected void ApplySteering()
    {
        if (sensors.isSensingObstacle) return;

        Vector3 relativeVector = transform.InverseTransformPoint(pathController.GetCurrentNodePosition(currentRoadNode.nodeNumber));
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
        targetSteerAngle = newSteer;
    }

    

	IEnumerator OnReverse(float seconds, bool steerAngleToZero)
	{
		status.isBraking = false;
		status.isReverse = true;
		wheelFR.brakeTorque = 0;
		wheelFL.brakeTorque = 0;
		OnReverse();

		if (steerAngleToZero)
		{
			targetSteerAngle = 0; //Should work?
		}

		yield return new WaitForSeconds(seconds);
		status.isReverse = false;

		yield return StartCoroutine(IEnumeratorGoAround());
		status.isStuck = false;
	}

    protected void Braking()
    {
        if (status.isBraking || status.keepBreaking)
        {
            if (!status.brakingLock) //prevent constant set until we get the statemachine
            {
                wheelFR.brakeTorque = maxBrakeTorgue;
                wheelFL.brakeTorque = maxBrakeTorgue;
                //sharedMaterialLights.mainTexture = textureBreaking;
                status.brakingLock = true;
            }
        }
        else
        {
            wheelFR.brakeTorque = 0;
            wheelFL.brakeTorque = 0;
            // materialLights.mainTexture = textureNormal;
            status.brakingLock = false;

        }
    }

    //Slows car and stops if nessecary
    protected IEnumerator BrakeUntil(RaycastHit hit)
	{
		status.isCarAheadCheckOnGoing = true;
		status.keepBreaking = true;
		status.isThrottle = false; // does nothing but makes me feel better

		yield return new WaitUntil(() =>
			hit.transform.GetComponent<BaseCar>() == null || //TODO: this should not be happening, FIX YOUR CODE!
			hit.transform.gameObject.GetComponent<BaseCar>().RigidBodySpeedKmH - 1 > this.RigidBodySpeedKmH  //Keep braking until you get lower speed OR some other method releases this loop with status.isCarAheadCheckOnGoing
            || status.isCarAheadCheckOnGoing == false);

		status.keepBreaking = false;
		status.isCarAheadCheckOnGoing = false;
	}

    #endregion

    #region Subsystems

    public virtual void SpeedoMeter()
    {
        if (RigidBodySpeedKmH > GetLaneSpeed(pathController.currentLaneIndex)) //Currently not working?
        {
            status.isSpeedLimiterOn = true;
            status.isBraking = true;
        }
        else
        {
            status.isBraking = false;
            status.isSpeedLimiterOn = false;
        }
    }

    #endregion

}
