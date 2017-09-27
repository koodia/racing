using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[System.Serializable]
//public class VehiclePrefab
//{
//    [SerializeField]
//    public CarPrefabName carPrefabName;
//    public CarCategory category;
//}

/// <summary>
/// TODO: Create baseclass for other spawners
/// </summary>
[System.Serializable]
public class VehicleSpawner : Singleton<VehicleSpawner>, ICargoCreator
{
    //temp stuff:
    public GameObject racerPrefab; //AIRacer currently
    //private TrafficWavePatterns wavePatterns = new TrafficWavePatterns();
    private CargoCreator cargoCreator = new CargoCreator();
    //public GameObject playerPrefab;

    public const string folderInResourcesVehicles = "Vehicles/";

    private Dictionary<string, GameObject> allSpawnedVehiclesSoFar = new Dictionary<string, GameObject>(); //TODO: for object pooling. And the list type?

    //[SerializeField]
    //public PrefabContainer container;
    //public GameObject[] containerVehicleList;
    //public VehiclePrefab[] vehicleList;
    //TODO: public PrefabList staticProps;
    //TODO: public PrefabList rigidProps;

    VehicleStyle vehicleStyle;
    private Transform trafficObject;
    private Transform racersObject;
    public List<GameObject> lanes;
    public int minDistanceBetweenVehicles = 40;
    public int maxDistanceBetweenVehicles = 120;
    private float currentInstantiationDistance;
    private const int instantiationHeight = 1;

    //These go to GameManager or something like that
    public static int totalNumberOfVehicles = 0;
    public static int numberOfVehiclesEmulated = 0;
    public static int numberOfCargoObjects;


    [SerializeField]
    public int spawnCarCountMin;
    [SerializeField]
    public int spawnCarCountMax;
    TrafficVehicleCreator tafficCreator = new TrafficVehicleCreator();
    //ItemCreator itemCreator = new ItemCreator();
    public Transform[] racers;

    //TODO://This will change when 3,2,1 Start! is implemented 
    private const int LaneStarts = 200;
    private int earlySpawningPos = LaneStarts; // WHAT EVER YOU DO NOOOOOOOOOOOOOOOOOOOOO NOT SET THIS TO ZERO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Unity has a bug
    public Vector3 startlineInstantiationPos = new Vector3(-600, 70f, 522); //this cumulates according the opponentCount. A BUG HERE AS WELL: You need to drop the cars high enough!!

    //Some temp aid:
    private bool laneSpawnLock = false;
    private float lockTimer = 20;
    private float rayCastCheckLength = 40;
    private int waitBeforeRelease = 0;
    private int tempCarCountBeginning = 0;

    private static int playerId = -1;

    void Awake()
    {
        lanes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Lane")).OrderBy(go => go.name).ToList();
    }

    void Start()
    {

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
        lockTimer -= Time.deltaTime;
        if (lockTimer < 0)
        {
            laneSpawnLock = false;
        }

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
            StartCoroutine(InstantiateVehicle(6, 500, CarPrefabName.MiniCargoTruck_Prefab)); //One test car
        }
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

        CarData.ModifyAIPrefabToPlayable(go, automaticTransmission, playerId);
    }

    private void InstantiateRacer(Vector3 currentInstantiationPos, out GameObject go)
    {
        int startingLane = 7;
        vehicleStyle = tafficCreator.PickRandomRaceVehicleAndStyle();
        go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, vehicleStyle.racerPrefabTempThing)), currentInstantiationPos, Quaternion.identity, racersObject) as GameObject;
        go.transform.eulerAngles = new Vector3(0, 90, 0);
        vehicleStyle.isSpecialColored = go.transform.GetComponent<BaseCar>().style.isSpecialColored; //Ugly but check the coloring from the prefab like this until we fix this
        tafficCreator.DecorateCarWithStyle(go, vehicleStyle);

 
        go.GetComponent<AIRacerController>().raceStatus.isStartingAtStartingLine = true;

        go.GetComponent<AIRacerController>().equipment = go.AddComponent<Equipment>(); //TESTAA
        go.GetComponent<AIRacerController>().equipment.Init(2);

        totalNumberOfVehicles++;
        go.name = "Racer " + totalNumberOfVehicles;
        go.GetComponent<PathController>().currentLaneIndex = startingLane;
        go.GetComponent<BaseCar>().status.isEngineOn = false;
        //go.GetComponent<AIRacerController>().RefreshRoadObject(); //TEST
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

        if (vehicleStyle.hasCargoSpace)
        {
            cargoCreator.InstantiateRandomCargo(go, goController.cargo);
        }

        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private IEnumerator InstantiateRandomVehicle(int laneIndex, float pos)
    {
        vehicleStyle = tafficCreator.PickRandomVehicle();
        pos += vehicleStyle.AddDistanceIfNeeded(ref currentInstantiationDistance); //ugly, but will do for now
        GameObject go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, vehicleStyle.prefabName.ToString())), new Vector3(lanes[laneIndex].transform.position.x, instantiationHeight, pos), Quaternion.identity) as GameObject;
        vehicleStyle.isSpecialColored = go.transform.GetComponent<BaseCar>().style.isSpecialColored; //Ugly but check the coloring from the prefab like this until we fix this
        tafficCreator.DecorateCarWithStyle(go, vehicleStyle);
        go.GetComponent<PathController>().currentLaneIndex = laneIndex;
        go.transform.parent = trafficObject;
        //go.GetComponent<AIBaseCar>().raceStatus.isStartingAtStartingLine = false; //TODO: remove this when you get normal traffic car prefabs
        totalNumberOfVehicles++;
        go.name = go.name.Replace("(Clone)", " " + totalNumberOfVehicles);
        TrafficVehicleController goController = go.GetComponent<TrafficVehicleController>();

        yield return new WaitForSeconds(1.5f); //Have to wait so the car is absolut still for the cargo
        //if (vehicleStyle.hasCargoSpace)
        //{
        //    InstantiateRandomCargo(go, goController.cargo);
        //}
        go.GetComponent<AIBaseCar>().status.isEngineOn = true;

        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private IEnumerator InstantiateRandomVehicleTempSolution(int laneIndex, float pos)
    {
        vehicleStyle = tafficCreator.PickRandomVehicle();
        if (vehicleStyle.prefabName == CarPrefabName.CargoTruck_Prefab | vehicleStyle.prefabName == CarPrefabName.Metrobus_Prefab) //Wait some extra time if long vehicle. RODO Later a better solution
        {
            yield return new WaitForSeconds(10);
        }
        pos += vehicleStyle.AddDistanceIfNeeded();
        GameObject go = Instantiate(Resources.Load(String.Format("{0}{1}", folderInResourcesVehicles, vehicleStyle.prefabName.ToString())), new Vector3(lanes[laneIndex].transform.position.x, instantiationHeight, pos), Quaternion.identity) as GameObject;
        TrafficVehicleController goController = go.GetComponent<TrafficVehicleController>();

        tafficCreator.DecorateCarWithStyle(go, vehicleStyle);
        go.GetComponent<PathController>().currentLaneIndex = laneIndex;
        go.transform.parent = trafficObject;
        goController.raceStatus.isStartingAtStartingLine = false; //TODO: remove this when you get normal traffic car prefabs
        totalNumberOfVehicles++;
        go.name = go.name.Replace("(Clone)", " " + totalNumberOfVehicles);
        go.GetComponent<AIBaseCar>().status.isEngineOn = true;

        yield return new WaitForSeconds(1.5f); //Have to wait so the car is absolut still for the cargo
        //if (vehicleStyle.hasCargoSpace)
        //{
        //    InstantiateRandomCargo(go, goController.cargo);
        //}

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
            //TODO: temp if, get rid off:
            //if (currentInstantiationDistance > VideoSettings.DefaultVisionRangeAtRoadLevel + 500) //+500, A bit more for testing 
            //{
            //    Debug.Log("PopulateLane visionRange exeeded. Rest of the cars wont be created. Cars left:" + (carCount - 1).ToString());
            //    yield break;
            //}
        }
        yield return null;
    }



    /// <summary>
    /// This is quite tricky. Dont go under 10 seconds or the raycast check will remove it
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartReleasingCars()
    {
        yield return new WaitForSeconds(1); //needed
        //earlySpawningPos = 0; //start the spawning pos  ////Creates a small time cap for the AI racers enter the motorway

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
        // CarPrefabName previousCar = CarPrefabName.Camaro_Prefab; //short car for default

        if (waitSeconds == -1)
        {
            waitBeforeRelease = UnityEngine.Random.Range(7, 12);
        }

        tempCarCountBeginning = UnityEngine.Random.Range(spawnCarCountMin, spawnCarCountMax); //car count on each lane
        for (int i = 0; i < tempCarCountBeginning; i++)
        {
            if (waitSeconds == -1)
            {
                waitBeforeRelease = My.rand.Next(7, 12); //truly random
            }

            yield return new WaitForSeconds(waitBeforeRelease); //TODO: SCRAP YOU PLAN or create table or each lane bool lane1free, bool lane2free.....
            if (IsPositionFreeForSpawning(laneIndex) == true)
            {
                yield return StartCoroutine(InstantiateRandomVehicleTempSolution(laneIndex, roadPosition)); //Temp solution
            }
        }
        yield return null;
    }


    public IEnumerator PopulateVisibleMotorway()
    {
        //Less in the opposite side
        yield return StartCoroutine(PopulateLane(0, earlySpawningPos));
        yield return StartCoroutine(PopulateLane(2, earlySpawningPos));
        yield return StartCoroutine(PopulateLane(3, earlySpawningPos));

        yield return StartCoroutine(PopulateLane(4, earlySpawningPos));
        yield return StartCoroutine(PopulateLane(5, earlySpawningPos));
        yield return StartCoroutine(PopulateLane(6, earlySpawningPos));
        yield return StartCoroutine(PopulateLane(7, earlySpawningPos));

        //Further as well
        //Add parameter for cor count to the lane
        currentInstantiationDistance = 0; //zero this

        //Some a bit further as well
        yield return StartCoroutine(PopulateLane(4, 4000));  //jump on this lane so, avoid it for now
        yield return StartCoroutine(PopulateLane(5, 2700));
        yield return StartCoroutine(PopulateLane(6, 2500));
        yield return StartCoroutine(PopulateLane(7, 3000));

        //TODO: release them later!
    }

    #endregion

    /// <summary>
    /// The cargo items must have kinematic = true;
    /// </summary>
    /// <param name="targetVehicle"></param>
    /// <param name="vehicleCargo"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public IEnumerator InstantiateItemInCargo(GameObject targetVehicle, Cargo vehicleCargo, int index)
    {
        var randomItem = cargoCreator.itemCreator.PickRandomItem().ToString();
        Debug.Assert(randomItem != null);
        try
        {
            GameObject go = Instantiate(Resources.Load(String.Format("{0}{1}", CargoCreator.folderInResourcesCargo, randomItem)),
            new Vector3(targetVehicle.transform.position.x + vehicleCargo.cargoPositions[index].x,
                        targetVehicle.transform.position.y + vehicleCargo.cargoPositions[index].y,
                        targetVehicle.transform.position.z + vehicleCargo.cargoPositions[index].z), Quaternion.identity) as GameObject;
            go.transform.parent = targetVehicle.transform.GetComponent<BaseCar>().visualsObjectInHierarchy.transform;

            vehicleCargo.AddCargoItem(go.transform.GetComponent<Item>()); //TODO: remove the GetComponent
        }
        catch (NullReferenceException e)
        {
            Debug.Log("FAILED!:" + randomItem);

        }

        //NOTE: the default is kinematic = true! at least should!
        yield return null;
    }
}