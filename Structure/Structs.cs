
using System;
using UnityEngine;

public struct GameSettings
{
    readonly GameMode gameMode;
    readonly int rubberEffect; //strength from 0 to 10;
    readonly bool permaDeath;
    readonly public static float deathTimer = 10;
    readonly public int laneWidth;
    readonly public int opponentCount;
    readonly public float maxSpeedInGame;
    readonly public float trafficSpeed;
    readonly public float gasStationSingleRecoveryCycleWaitTime;

    public GameSettings(GameMode gameMode, int rubberEffect, bool permaDeath)
	{
		this.gameMode = gameMode;
		this.rubberEffect = rubberEffect;
		this.permaDeath = permaDeath;
        this.laneWidth = 0; // TOOD determine this later
        this.maxSpeedInGame = 400; //for now
        this.trafficSpeed = 60;
        this.opponentCount = 9;
        this.gasStationSingleRecoveryCycleWaitTime = 1;
    }
}

[System.Serializable]
public struct CarSpecs //struct or class....the cars should use reference when instantiating 
{
    
    public string vehicleName;
    public string description;
    public CarCategory carCategory;
    public int topSpeed;       //There is no speed limit in the game, but the acceleration will be so mild at high speeds. This attributes makes it more easier to achieve higher speeds at high gears
    public int acceleration;
    public int torgue;         //might be too hard to implement, we might drop this
    public int braking;
    public int handling;
    public int grip;           //Unrelevant sliding when turning example switching fast from lane to lane 
    public ArmorLevel armor;          //How much beating can the car hadnle until it gets destroyed
    public int mass;           //How easy the car c
    public int weaponSlots;     //How many weapons can you carry in inventory. Zero means none. PowerUps != weapons
                               //public int fuelCapacity; //Just maybe for the endurance mode
    //Hidden:
    //public int twitchiness;    //Nearly indentical with sideSlip. How quickly the car might spin"kill you" when accelerated wrongly. The effect is mostly experienced when you lose car traction and accelerate strongly or vica versa
    //public int slip;           //Forward tire slip when accelerating heavily "lag" before the car really starts to accelerate
    //public int sideSlip;       //Random sideway tire slip when accelerating 
}

[System.Serializable]
public struct LaneDimensions
{
    public int height;
    public int width;  //Currently only 2 widths, wide highway and normal
}

/// <summary>
/// TODO: Some of these actually affect the game logic
/// </summary>
[System.Serializable]
public struct DrivingStatus
{
    //[UnityEngine.Header("Status")]
    [ReadOnly] public bool isEngineOn;
    public bool IsEngineOn
    {
        get { return isEngineOn; }
        set { isEngineOn = value; }
    }

    [ReadOnly] public bool isThrottle;
    [ReadOnly] public bool isBraking;
    [ReadOnly] public bool isReverse;
    [ReadOnly] public bool brakingLock;
    [ReadOnly] public bool isPanicBraking;
    [ReadOnly] public bool isSlipping;
    [ReadOnly] public bool isLookingBehind;
     public bool isInTrouble;
    [ReadOnly] public int stuckCounter;
    [ReadOnly] public bool isSpeedLimiterOn;
    [ReadOnly] public bool isStuck;
    [ReadOnly] public bool keepBreaking;
    [ReadOnly] public bool isCarAheadCheckOnGoing;
    [ReadOnly] public bool boolUpsideDown;

    //public bool isAccelerate;
    //public bool isBraking;
    //public bool isReverse;
    //public bool brakingLock;
    //public bool isPanicBraking;
    //public bool isSlipping;
    //public bool isLookingBehind;
    //public bool isInTrouble;
    //public int stuckCounter;
    //public bool isSpeedLimiterOn;
    //public bool isStuck;
    //public bool keepBreaking;
    //public bool isCarAheadCheckOnGoing;
    //public bool boolUpsideDown;

    public void SetAllBoolsToFalse()
    {
        isEngineOn = false;
        isThrottle = false;
        isBraking = false;
        isReverse = false;
        brakingLock = false;
        isPanicBraking = false;
        isSlipping = false;
        isLookingBehind = false;
        isInTrouble = false;
        isSpeedLimiterOn = false;
        isStuck = false;
        keepBreaking = false;
        isCarAheadCheckOnGoing = false;
    }
}

[System.Serializable]
public struct RaceStatus
{
    public bool isStartingAtStartingLine;
    [ReadOnlyAttribute] public int position; //calculated from roadNode?
    [ReadOnlyAttribute] public bool gameOver;
    [ReadOnlyAttribute] public bool roadEnds;
}

[System.Serializable]
public struct Sensors
{
    //[Header("Sensors")]
    public float sensorLength;
    public float frontSideSensorPosX;
    public float frontSensorAngle;
    public Vector3 frontSensorPosition;
    public float frontTopSensorHeight;
    public bool isSensingObstacle;
    public bool isfrontSensingObstacle;
   
    /// <summary>
    /// Currently same length for all vehicles
    /// </summary>
    /// <param name="category"></param>
    public Sensors(CarCategory category)
    {
        switch (category)
        {
            case CarCategory.Muscle:
            case CarCategory.Guns:
            case CarCategory.HeavyDuty: //Add more while you make it
            case CarCategory.Racing: //Add more while you make it
            case CarCategory.CargoCarrier: //Add more while you make it
                sensorLength = 100;
            frontSideSensorPosX = 4.2f;
            frontSensorAngle = 25;
            frontSensorPosition = new Vector3(0, 2f, 5.5f); //TODO:capture from box collider or something
            frontTopSensorHeight = 10f;
            isSensingObstacle = false;
            isfrontSensingObstacle = false;
            break;
            default: throw new Exception("TODO! The category does not have sensors yet!:" + category);
        }
    }
}

