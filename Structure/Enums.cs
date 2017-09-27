
    public enum LaneType
    {
        MotorwayLane //No maximum
        ,NormalLane //max 100km/h
        ,Sideroad //max 60km/h
        ,Offroad
    }

    public enum RoadType
    {
        Tarmac
        ,OldTarmac
        ,Sand
    }

    public enum SectionType
    {
	    Straight
	    ,Turn
	    ,Corner
	    ,LaneGoesAlongX
    }

    public enum SensorPlacement
    {
        FrontMiddle
        , FrontSide
        , Side
        , Angle
        , Roof
    }

    public enum CarCategory
    {
        NotSet
        ,Racing
        ,Supercar
        ,Turbo
        ,Muscle
        ,Sport
        ,Guns
        ,ShoppingBag //"Small" Hatchback
        ,CargoCarrier //Extra hard springs
        ,HeavyDuty
    }

    /// <summary>
    /// All possible traffic vehicles
    /// </summary>
    public enum CarPrefabName // "Model"
    {
         Metrobus_Prefab
        ,MiniCargoTruck_Prefab
        ,DodgeRam_Prefab    //Playable
        ,Camaro_Prefab  //Playable
        ,CargoTruck_Prefab
        ,L200Pickup_Prefab
        ,OldBeetle_Prefab
        , Ferrari_Prefab  //Playable
        //TODO:,PoliceCar
    }

    public enum ItemPrefab
    {
          OilBarrel_Prefab
        , ToxicBarrel_Prefab
        , RadiationBarrel_Prefab
        , BenzinBarrel_Prefab
        , MachineGunCrate_Prefab
        , MissileCrate_Prefab
        , NitroCrate_Prefan
        , WoodCrate_Prefab
        //, WoodCrateBig_Prefab
    //, ExlosiveBarrel_Prefab
}

    public enum AbilityPrefab
    {
        Missile_Prefab
        ,MachineGun_Prefab
        ,Nitro_Prefab

        ,ToDO
        ,MissileBullet_Prefab
        ,MachineGunBullet_Prefab
        ,NitroBoost_Prefab //does not have a prefab yet, maybe never
        ,QualityRoad_Prefab //does not have a prefab yet, maybe never
    //SlowDownMissile
    //Rope
    //Shield
    //IceBeam
    //OverCharger (shock)
    //Jump springs
}

   
    /// <summary>
    /// Automatically used
    /// </summary>
    public enum PowerUp
    {
        //PowerUps
        NitroBoost
        ,QualityRoad       //activated on new tarmac
        ,NoMorePolice
        ,AllEngineShutdown   // opponents engines need to be restarted
        ,TopGearsMissing     // opponents cannot change gear above 3
        ,OilOnTheRoad        // slippery tyres to opponents
        ,Sabotage            // reversed controls
        ,CallPolice          // cops on opponents' tail
        ,MoreMass            // heavier opponents and slower
        ,BountyHunter        // the leader gets a gunning car on its tail for the rest of the game
        ,RushOur             // the front cars will experience some major traffic which is very tricky drive through
    }

    public enum Effect
    {
        PressureWave
        , Explosion
        , Fire
        , Oil
        , BlurView
        , DamageCloud
    }

    public enum DriveStyle
    {
        Normal          //default
        ,Hurry          //Like normal but drives 10% to 40% faster than the rest of the traffic
        ,LearnerDriver  //Does unthinkable, drives with too small gear and car engine screams. Also might even stop middle of the road
        ,Hooligan       //Drives fast, ises turn signal rarely
        ,Speeder        //Like normal but no speed limit
        ,MadMan         //If you hit, the will chase you and try to ram you over
        ,Sundaydriver   //Annoyiance, some granny with 10% to 40% lower speed
    }

    public enum WaveType
    {
        NearEmpty       // 0 to 5 cars
        ,Casual         // 10 to 20 cars
        ,Slow           // 10 to 50 cars
        ,Tricky         // 10 to 50 cars PLUS specially long vehicles which dont enable lane swithing that easily
    }

    public enum DrivePathInTraffic //WaveForm
    {
        Horizontal
        ,Vertical
        ,Diagonal
    }

    public enum GameMode
    {
        Tournament      //Race to the finish
        ,SpeedRun       //solo TimeTrial
        ,Chase          //Destroy car first
        ,Civilized      //No guns
    }

    /*Strong side to side bash = medium hit(3 damage)
     * Check damage values from Armor.TakeHitOrDamage method 
    */
    public enum ArmorLevel
    {
        NotSet = 0
        ,Fragile = 7    //Formula: 2 medium hits + minorhit
        ,Weak = 15      //Sport car: 5 medium hits
        ,Medium = 45    //Muscle car: 15 medium hits
        ,Strong = 60    //Pickup: 20 medium hits
        ,HeavyDuty = 120 //Lorry car: 40 medium hits
    }

    public enum WheelType
    {
         FWD
        ,RWD
        ,AWD
        ,Custom
    }

	public enum CameraView
	{
		BonnetView
		,Frontview
		,FirstPerson
        ,ThirdPerson
        //TODO CockpitView ,possibly that is 
    }

    public enum Bonus
    {
          DestroyRacer
        , First
        , Second
        , Third
        , Jumbo
        , NoHits
        , Roll
        , DeathRoll
        , ThreeSixty
        , MoneyBag
        , SpeedRecord
        , ChaosRecord
        , HighJump
        , FastestPassBy
        , LongestWrongSideDrive
        , MadMax
    }
