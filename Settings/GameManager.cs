
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


 public sealed class GameManager :  Singleton<GameManager>
{
    public AudioSource gameOverMusic;
    private AudioSource finishMusic;
    public GameSettings settings;
    public GameplayBalancer balancer;
    public VehicleSpawner vehicleSpawner; //set to private
 
    public int opponentCount = 0; // plus player
    public bool automaticTransmission = false;
    //Todo LevelLoader level 

    [Header("Debug switches")]
    public bool debugTraffic;
    public bool populaLateMotorway;
    public bool startReleasingTraffic;
    public bool race;
    public static bool dontEmulateTraffic;
    private float startTimer = 4;
    public string racerPrefabToUse;

    private bool unitTesting = false; //someway to test

    void Start()
    {
        if (unitTesting) //hack
        {
            InitGame(false, false, true, "RacerCamaro_Prefab");
        }
        else
        {
            InitGame(populaLateMotorway, startReleasingTraffic, race, racerPrefabToUse);
            StartCoroutine(ReleaseRacersWith123GO());
        }
    }

    IEnumerator ReleaseRacersWith123GO()
    {
        yield return new WaitForSeconds(2);

        while (startTimer > -1)
        {
            startTimer -= Time.deltaTime;
            if (startTimer < 1)
            {
                HUD.instance.releaseTimerText.text = (int)startTimer == 0 ? "GO!" : ((int)startTimer).ToString();
                for (int i = 0; i < vehicleSpawner.racers.Length; i++)
                {
                    vehicleSpawner.racers[i].GetComponent<BaseCar>().status.isEngineOn = true;
                }
                StartCoroutine(HideGO());
                break;
            }
            else
            {
                HUD.instance.releaseTimerText.text = ((int)startTimer).ToString();
                yield return null;
            }
        }
    }

    IEnumerator HideGO()
    {
        yield return new WaitForSeconds(1);
        HUD.instance.releaseTimerText.text = "";
        HUD.instance.releaseTimerText.gameObject.SetActive(false);

        yield return null;
    }

    public void DisplayGameOver()
    {
        gameOverMusic.Play();
    }

    public void FinishRace()
    {
        //finishMusic.Play();
    }

    public void InitGame(bool populaLateMotorway, bool startReleasingTraffic, bool race, string racerPrefabToUse)
    {
        Debug.Assert(!string.IsNullOrEmpty(racerPrefabToUse), "racerPrefabToUse cannot be empty");

        settings = new GameSettings(GameMode.Tournament, 5, false);
        balancer = new GameplayBalancer(0, 500, 100, 400);
        dontEmulateTraffic = debugTraffic;//helps testing


        vehicleSpawner = gameObject.GetComponent<VehicleSpawner>();
        if (vehicleSpawner == null)
        {
            if (unitTesting)
            {
                vehicleSpawner = gameObject.AddComponent<VehicleSpawner>();
                gameObject.AddComponent<UnityInputManager>();
            }
            else
            {
                throw new System.NullReferenceException("VehicleSpawner not attached to the gameObject");
            }
        }
        
        vehicleSpawner.racers = new Transform[opponentCount + 1]; //plus player
        if (race)
        {
            vehicleSpawner.InstantiateRacers(opponentCount);
            vehicleSpawner.InstantiatePlayer(racerPrefabToUse, vehicleSpawner.startlineInstantiationPos, automaticTransmission); //player will be at last pos
        }

        if (populaLateMotorway)
        {
            StartCoroutine(vehicleSpawner.PopulateVisibleMotorway());
          
        }

        if (startReleasingTraffic)
        {
            StartCoroutine(vehicleSpawner.StartReleasingCars());
        }
    }

}
