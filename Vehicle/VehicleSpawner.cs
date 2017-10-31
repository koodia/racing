using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// TODO: Create baseclass for other spawners
/// </summary>
[System.Serializable]
public class VehicleSpawner : Singleton<VehicleSpawner> //ICargoCreator
{
    //temp stuff:
    public GameObject racerPrefab; //AIRacer currently
 
    //public GameObject playerPrefab;

    public const string folderInResourcesVehicles = "Vehicles/";

    private Dictionary<string, GameObject> allSpawnedVehiclesSoFar = new Dictionary<string, GameObject>(); //TODO: for object pooling. And the list type?
    VehicleStyle vehicleStyle;
    private Transform trafficObject;
    private Transform racersObject;
    public List<GameObject> lanes;
    public int minDistanceBetweenVehicles = 40;
    public int maxDistanceBetweenVehicles = 120;
    private float currentInstantiationDistance;
    private const int DefaultInstantiationHeight = 1;
    private float instantiationHeight = DefaultInstantiationHeight;

    //These go to GameManager or something like that
    public static int totalNumberOfVehicles = 0;
    public static int numberOfVehiclesEmulated = 0;
    public static int numberOfCargoObjects;


    [SerializeField]
    public int spawnCarCountMin;
    [SerializeField]
    public int spawnCarCountMax;
    TrafficVehicleCreator tafficCreator = new TrafficVehicleCreator();
    public Transform[] racers;

    //TODO://This will change when 3,2,1 Start! is implemented 
    private const int LaneStarts = 200;
    private const int LanePopulationPosStart = 400;
    private int earlySpawningPos = LaneStarts; // Unity Canvas has a bug, so dont set to zero
    public Vector3 startlineInstantiationPos = new Vector3(-600, 70f, 522);
    private float rayCastCheckLength = 40;
    private int waitBeforeRelease = 0;
    private int tempCarCountBeginning = 0;
    private static int playerId = -1;

    //Cargo creation:
    CargoCreator cargoCreator = new CargoCreator();


    public override void Awake()
    {
        base.Awake();
        lanes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Lane")).OrderBy(go => go.name).ToList();
    }

    void Start()
    {
        Debug.Assert(spawnCarCountMax != 0, "VehicleSpawner's spawnCarCountMax needs to be greater than 0 ");
        Init();
    }

    public void Init()
    {
        trafficObject = GameObject.Find("Traffic").transform;
        Debug.Assert(trafficObject != null, "trafficObject");

        racersObject = GameObject.Find("Racers").transform;
        Debug.Assert(trafficObject != null, "traffic");
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(PopulateLane(0, earlySpawningPos));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(PopulateLane(1, earlySpawningPos));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(PopulateLane(2, earlySpawningPos));
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(PopulateLane(3, earlySpawningPos));
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            StartCoroutine(PopulateLane(4, earlySpawningPos));
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            StartCoroutine(PopulateLane(5, earlySpawningPos));
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            StartCoroutine(PopulateLane(6, earlySpawningPos));
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            StartCoroutine(PopulateLane(7, earlySpawningPos));
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            StartCoroutine(InstantiateVehicle(6, 500, CarPrefabName.CargoTruck_Prefab)); //One test car
        }
    }

      /// <summary>
    /// This is quite tricky. Dont go under 10 seconds or the raycast check will remove it
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartReleasingCars()
    {
        yield return new WaitForSeconds(1); //needed

        //Less in the opposite side, meaning rarely
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 0));
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 1));
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 2));
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 3));

        //remember to wait a sec
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 4));
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 5));
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 6));
        StartCoroutine(ReleaseWaveRoadBeginning(earlySpawningPos, WaveType.Casual, 7));

        yield return null;
    }


    public IEnumerator PopulateVisibleMotorway()
    {
        //Less in the opposite side
        yield return StartCoroutine(PopulateLane(0, LanePopulationPosStart));
        yield return StartCoroutine(PopulateLane(2, LanePopulationPosStart));
        yield return StartCoroutine(PopulateLane(3, LanePopulationPosStart));

        yield return StartCoroutine(PopulateLane(4, LanePopulationPosStart));
        yield return StartCoroutine(PopulateLane(5, LanePopulationPosStart));
        yield return StartCoroutine(PopulateLane(6, LanePopulationPosStart));
        yield return StartCoroutine(PopulateLane(7, LanePopulationPosStart));

        //Further as well
        //Add parameter for cor count to the lane
        currentInstantiationDistance = 0; //zero this

        //Some a bit further as well
        yield return StartCoroutine(PopulateLane(4, 4000));  //jump on this lane so, avoid it for now
        yield return StartCoroutine(PopulateLane(5, 2700));
        yield return StartCoroutine(PopulateLane(6, 2500));
        yield return StartCoroutine(PopulateLane(7, 3000));

        yield return StartCoroutine(PopulateLane(4, 6000));  //jump on this lane so, avoid it for now
        yield return StartCoroutine(PopulateLane(5, 4500));
        yield return StartCoroutine(PopulateLane(6, 5000));
        yield return StartCoroutine(PopulateLane(7, 5100));

        //TODO: release them later!
    }

    /// <summary>
    /// The vectors should be cached or just use boxCollider. BTW destoys player as well at the moment =)
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <returns></returns>
    public bool IsPositionFreeForSpawning(int laneIndex)
    {
        RaycastHit hit;
        Vector3 sensorStartPos;
        bool laneLeftSideFree = true;
        bool laneRightSideFree = true;

        sensorStartPos = new Vector3(lanes[laneIndex].transform.position.x + 5, instantiationHeight - 0.5f, earlySpawningPos - 5);
        //Debug.DrawLine(sensorStartPos, new Vector3(lanes[laneIndex].transform.position.x + 5, instantiationHeight, earlySpawningPos + rayCastCheckLength), Color.red, 1);
        if (Physics.Raycast(sensorStartPos, new Vector3(lanes[laneIndex].transform.position.x + 5, instantiationHeight - 0.5f, rayCastCheckLength), out hit)) //uses 8/16 signature
        {
            if (!hit.collider.transform.parent.CompareTag("Terrain") &&
                  hit.transform.name.Contains("CargoTruck_Prefab") == false) //TODO: create faster solution for long vehicles
            {
                //Debug.Log(hit.transform.name);
                laneLeftSideFree = false;
                hit.transform.gameObject.SetActive(false); //remove move it from the way so it wont do any harm
            }
        }
        sensorStartPos = new Vector3(lanes[laneIndex].transform.position.x - 5, instantiationHeight - 0.5f, earlySpawningPos - 5);
        // Debug.DrawLine(sensorStartPos, new Vector3(lanes[laneIndex].transform.position.x - 5, instantiationHeight, earlySpawningPos + rayCastCheckLength), Color.blue,1);
        if (Physics.Raycast(sensorStartPos, new Vector3(lanes[laneIndex].transform.position.x - 5, instantiationHeight - 0.5f, rayCastCheckLength), out hit)) //uses 8/16 signature
        {
            if (!hit.collider.transform.parent.CompareTag("Terrain") &&
                hit.transform.name.Contains("CargoTruck_Prefab") == false) //TODO: create faster solution for long vehicles
            {
                Debug.Log(hit.transform.name);
                laneRightSideFree = false;
                hit.transform.gameObject.SetActive(false); //remove move it from the way so it wont do any harm
            }
        }

        if (!laneLeftSideFree | !laneRightSideFree)
        {

            return false;
        }

        return true;
    }

    /// <summary>
    /// TODO: remove hardcodes values!
    /// </summary>
    /// <param name="racerCount"></param>
    public void InstantiateRacers(int racerCount)
    {
        //racers = new Transform[racerCount];
        bool populatingNextLane = false;
        GameObject go = null;

        //Vector3 currentInstantiationPos = new Vector3(-600, 45f, 522);
        for (int i = 0; i < racerCount; i++)
        {
            if (i >= racerCount / 2 + 1 && populatingNextLane == false) //switch lane to spawn
            {
                populatingNextLane = true;
                startlineInstantiationPos.x = -600; //star from beginning
                startlineInstantiationPos.z = 545;
                startlineInstantiationPos.y = 45;
            }

            go = null;
            InstantiateRacer(startlineInstantiationPos, out go);
            racers[i] = go.transform;
            startlineInstantiationPos.x -= 40; //distance between racers at the start. The first corner would need some distance between the cars as well... add something to the roadNode?
        }
    }

    public void InstantiatePlayer(string carPrefabName, Vector3 pos, bool automaticTransmission)
    {
        playerId++;
        GameObject go;
        //And not forget the player:
        go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, carPrefabName)), pos, Quaternion.identity, racersObject) as GameObject;
        go.transform.eulerAngles = new Vector3(0, 90, 0);
        racers[racers.Length - 1] = go.transform; //player starts at last position

        PlayerController player = go.AddComponent<PlayerController>();
        player.TransformAIRacerPrefabToPlayable(go, automaticTransmission, playerId);
    }

    private void InstantiateRacer(Vector3 racersInstantiationPos, out GameObject go)
    {
        int startingLane = 7;
        vehicleStyle = tafficCreator.PickRandomRaceVehicleAndStyle();

        go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, vehicleStyle.racerPrefabTempThing)), racersInstantiationPos, Quaternion.identity, racersObject) as GameObject;

        go.transform.eulerAngles = new Vector3(0, 90, 0);
        vehicleStyle.isSpecialColored = go.transform.GetComponent<BaseCar>().style.isSpecialColored; //Ugly but check the coloring from the prefab like this until we fix this
        tafficCreator.DecorateCarWithStyle(go, vehicleStyle);

        var cntrl = go.GetComponent<AIRacerController>();
        cntrl.raceStatus.isStartingAtStartingLine = true;

        cntrl.equipment = go.AddComponent<Equipment>();
        VehicleSpecs specs = new VehicleSpecs();
        cntrl.equipment.Init(specs.GetVehicleSpecs(vehicleStyle.prefabName).weaponSlots);
        cntrl.cargo.SetCarsSpecificCargoSettings(vehicleStyle.prefabName);

        totalNumberOfVehicles++;
        go.name = "Racer " + totalNumberOfVehicles;
        go.transform.GetComponent<BaseCar>().temporaryWorkAroundRacerId = totalNumberOfVehicles - 1; //temp hack 
        cntrl.Racer = new Racer(totalNumberOfVehicles - 1, "Dirty Harry", "Y0U-L0S3", true);
        go.GetComponent<PathController>().currentLaneIndex = startingLane;
        go.GetComponent<BaseCar>().status.isEngineOn = false;
    }

    /// <summary>
    /// Instantiates a random car to a target lane
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <returns></returns>
    private IEnumerator InstantiateVehicle(int laneIndex, float pos, CarPrefabName carPrefab)
    {
        vehicleStyle = tafficCreator.PickVehicle(carPrefab);
        GameObject go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, vehicleStyle.prefabName.ToString())), new Vector3(lanes[laneIndex].transform.position.x, instantiationHeight, pos), Quaternion.identity) as GameObject;

        vehicleStyle.isSpecialColored = go.transform.GetComponent<BaseCar>().style.isSpecialColored; //Ugly but check the coloring from the prefab like this until we fix this
        tafficCreator.DecorateCarWithStyle(go, vehicleStyle);
        go.GetComponent<PathController>().currentLaneIndex = laneIndex;
        go.transform.parent = trafficObject;
        currentInstantiationDistance += UnityEngine.Random.Range(minDistanceBetweenVehicles, maxDistanceBetweenVehicles); //returns the "nextDistance"
        totalNumberOfVehicles++;
        go.name = go.name.Replace("(Clone)", " " + totalNumberOfVehicles);
        go.GetComponent<AIBaseCar>().status.isEngineOn = true;
        TrafficVehicleController goController = go.GetComponent<TrafficVehicleController>();
        goController.cargo.SetCarsSpecificCargoSettings(vehicleStyle.prefabName);

        yield return new WaitForSeconds(1.0f);
        if (vehicleStyle.HasCargoSpace)
        {
            InstantiateRandomCargo(go, goController.cargo);
        }

        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private IEnumerator InstantiateRandomVehicle(int laneIndex, float posZ)
    {
        vehicleStyle = tafficCreator.PickRandomVehicle();
        posZ += vehicleStyle.AddDistanceIfNeeded(ref currentInstantiationDistance); //ugly, but will do for now
        instantiationHeight = DefaultInstantiationHeight;
        instantiationHeight = vehicleStyle.FixHeightIfNeeded(instantiationHeight);

        GameObject go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, vehicleStyle.prefabName.ToString())), new Vector3(lanes[laneIndex].transform.position.x, instantiationHeight, posZ), Quaternion.identity) as GameObject;
        TrafficVehicleController goController = go.GetComponent<TrafficVehicleController>();
        vehicleStyle.isSpecialColored = go.transform.GetComponent<BaseCar>().style.isSpecialColored; //Ugly but check the coloring from the prefab like this until we fix this
        tafficCreator.DecorateCarWithStyle(go, vehicleStyle);
        go.GetComponent<PathController>().currentLaneIndex = laneIndex;
        go.transform.parent = trafficObject;
        totalNumberOfVehicles++;
        go.name = go.name.Replace("(Clone)", " " + totalNumberOfVehicles);
        goController.cargo.SetCarsSpecificCargoSettings(vehicleStyle.prefabName); //TODO: possibly preload into prefab?

        yield return new WaitForSeconds(1.0f);
        if (vehicleStyle.HasCargoSpace)
        {
            InstantiateRandomCargo(go, goController.cargo);
        }

        goController.GetComponent<AIBaseCar>().status.isEngineOn = true;

        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private IEnumerator InstantiateRandomVehicleTempSolution(int laneIndex, float posZ)
    {
        vehicleStyle = tafficCreator.PickRandomVehicle();
        if (vehicleStyle.prefabName == CarPrefabName.CargoTruck_Prefab | vehicleStyle.prefabName == CarPrefabName.Metrobus_Prefab) //Wait some extra time if long vehicle. RODO Later a better solution
        {
            yield return new WaitForSeconds(10);
        }
        posZ += vehicleStyle.AddDistanceIfNeeded();
        instantiationHeight = DefaultInstantiationHeight;
        instantiationHeight = vehicleStyle.FixHeightIfNeeded(instantiationHeight);
        GameObject go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, vehicleStyle.prefabName.ToString())), new Vector3(lanes[laneIndex].transform.position.x, instantiationHeight, posZ), Quaternion.identity) as GameObject;
        TrafficVehicleController goController = go.GetComponent<TrafficVehicleController>();

        tafficCreator.DecorateCarWithStyle(go, vehicleStyle);
        go.GetComponent<PathController>().currentLaneIndex = laneIndex;
        go.transform.parent = trafficObject;
        goController.raceStatus.isStartingAtStartingLine = false;
        totalNumberOfVehicles++;
        go.name = go.name.Replace("(Clone)", " " + totalNumberOfVehicles);
        go.GetComponent<AIBaseCar>().status.isEngineOn = true;
        goController.cargo.SetCarsSpecificCargoSettings(vehicleStyle.prefabName);


        yield return new WaitForSeconds(1.0f);
        if (vehicleStyle.HasCargoSpace)
        {
            InstantiateRandomCargo(go, goController.cargo);
        }

        goController.status.isEngineOn = true;
        yield return null;
    }

    #region releaseCars

    private IEnumerator ReleaseWave(float roadPosition, WaveType waveType, int laneIndex, float waitSeconds)
    {
        if (waitSeconds == -1)
        {
            waitBeforeRelease = UnityEngine.Random.Range(10, 16);
        }

        int carCount = UnityEngine.Random.Range(spawnCarCountMin, spawnCarCountMax); //car count on each lane
        for (int i = 0; i < carCount; i++)
        {
            yield return new WaitForSeconds(waitBeforeRelease);
            StartCoroutine(InstantiateRandomVehicleTempSolution(laneIndex, roadPosition)); //Temp solution
        }
    }

    IEnumerator PopulateLane(int laneIndex, int startPos)
    {
        int carCount = UnityEngine.Random.Range(spawnCarCountMin, spawnCarCountMax); //car count of each lane can be limited 
        currentInstantiationDistance = startPos;
        for (int i = 0; i < carCount; i++)
        {
            StartCoroutine(InstantiateRandomVehicle(laneIndex, currentInstantiationDistance));
            currentInstantiationDistance += UnityEngine.Random.Range(minDistanceBetweenVehicles, maxDistanceBetweenVehicles);
        }
        yield return null;
    }

  

    /// <summary>
    /// Todo when time
    /// </summary>
    /// <param name="roadPosition"></param>
    /// <param name="waveType"></param>
    /// <param name="laneIndex"></param>
    /// <returns></returns>
    private IEnumerator ReleaseWaveAdvanced(float roadPosition, WaveType waveType, int laneIndex)
    {
        switch (waveType)
        {
            case WaveType.Casual:
                break;
            case WaveType.NearEmpty:
                break;
            case WaveType.Slow:
                break;
            case WaveType.Tricky:
                StartCoroutine(ThreeLevelWaveForm(4));
                break;
            default: throw new System.NotImplementedException("waveType");
        }

        yield return null;
    }

    private IEnumerator ThreeLevelWaveForm(int timeCapBetweenLayers)
    {
        int carLayers = 3; // 3 to 6 levels
        for (int i = 0; i < carLayers; i++)
        {
            float roadPosition = 400;
            StartCoroutine(InstantiateRandomVehicle(4, roadPosition));
            StartCoroutine(InstantiateRandomVehicle(5, roadPosition));
            StartCoroutine(InstantiateRandomVehicle(6, roadPosition));
            StartCoroutine(InstantiateRandomVehicle(7, roadPosition));
            yield return new WaitForSeconds(timeCapBetweenLayers);
        }
        yield return null;
    }

    /// <summary>
    /// Same as ReleaseWave but does additional checking that the lane position is free
    /// </summary>
    /// <param name="roadPosition"></param>
    /// <param name="waveType"></param>
    /// <param name="laneIndex"></param>
    /// <param name="waitSeconds"></param>
    /// <returns></returns>
    private IEnumerator ReleaseWaveRoadBeginning(float roadPosition, WaveType waveType, int laneIndex, float waitSeconds = -1)
    {
        if (waitSeconds == -1)
        {
            waitBeforeRelease = UnityEngine.Random.Range(7, 12);
        }

        tempCarCountBeginning = UnityEngine.Random.Range(spawnCarCountMin, spawnCarCountMax); //car count on each lane
        for (int i = 0; i < tempCarCountBeginning; i++)
        {
            if (waitSeconds == -1)
            {
                waitBeforeRelease = My.rand.Next(7, 12);
            }

            yield return new WaitForSeconds(waitBeforeRelease); //TODO: SCRAP YOU PLAN or create table or each lane bool lane1free, bool lane2free.....
            if (IsPositionFreeForSpawning(laneIndex) == true)
            {
                yield return StartCoroutine(InstantiateRandomVehicleTempSolution(laneIndex, roadPosition)); //Temp solution
            }
        }
        yield return null;
    }

    #endregion

    /// <summary>
    /// The cargo items must have kinematic = true;
    /// </summary>
    /// <param name="targetVehicle"></param>
    /// <param name="vehicleCargo"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public IEnumerator InstantiateItemInCargo(GameObject targetVehicle, Cargo vehicleCargo, int index, bool mightBePowerUpItem)
    {
        string randomItem = "";
        if (mightBePowerUpItem)
        {
            randomItem = cargoCreator.PickItem(vehicleCargo, true).ToString();
        }
        else
        {
            randomItem = cargoCreator.PickItem(vehicleCargo, false).ToString();
        }
        Debug.Assert(randomItem != "", "string is empty");

        try
        {
            GameObject go = Instantiate(Resources.Load(String.Format("{0}{1}", CargoCreator.folderInResourcesCargo, randomItem)),
            new Vector3(targetVehicle.transform.position.x + vehicleCargo.cargoPositions[index].x,
                        targetVehicle.transform.position.y + vehicleCargo.cargoPositions[index].y,
                        targetVehicle.transform.position.z + vehicleCargo.cargoPositions[index].z), Quaternion.identity) as GameObject;
            go.transform.parent = targetVehicle.transform.GetComponent<BaseCar>().visualsObjectInHierarchy.transform;

            vehicleCargo.AddCargoItem(go.transform.GetComponent<Item>());
        }
        catch (NullReferenceException e)
        {
            Debug.Log("FAILED!:" + randomItem);
        }

        //NOTE: the default is kinematic = true! at least should!
        yield return null;
    }

    int itemCounter;
    public void InstantiateRandomCargo(GameObject targetVehicle, Cargo vehicleCargo)
    {

        bool mightBePowerUpItem = true;
        int cargoCount = 0;
        try
        {
            cargoCount = vehicleCargo.GetItemCount();
        }
        catch(Exception e)
        {
           //TODO: High likely its the vehicle terminator
           Debug.Log(targetVehicle.name + "FAILEEDDDDDDDDDDDD");
           return;
        }
      
        //The never ending chewingum: Alteration ultil spring balance issue is fixed!
        if (targetVehicle.GetComponent<AIBaseCar>().style.prefabName == CarPrefabName.MiniCargoTruck_Prefab)
        {
            if (cargoCount % 2 != 0)
            {
                return;
            }
        }

        itemCounter = 0;
        for (int i = 0; i < cargoCount; i++) //create a coroutine?
        {
            if (itemCounter >= vehicleCargo.numberOfPowerUps)
            {
                mightBePowerUpItem = false;
            }
           
            StartCoroutine(InstantiateItemInCargo(targetVehicle, vehicleCargo, i, mightBePowerUpItem));
            itemCounter++;
            numberOfCargoObjects++;
        }
    }
}

