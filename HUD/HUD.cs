using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    Transform[] racers;
    private GameObject controller;
    private Transform player1;
    private PlayerController playerController;
    private int playerHashCode;
    private float playerSpeed;
    public Text position;
    public Text vehicleCount;
    public Text speed;
    public Text currentGear;

    public Text finishRaceText;
    public Text gameOverText;
    public Text releaseTimerText;
    public Text upperNotfications;
    bool showingNotification;
    private const float notificationDisplayTime = 3;

    public Image temperatureImage;
    public Color32 imageColor;
    public static HUD instance;
    private float hudUpdatingFrequency = 2f;
    private float time;
    

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        imageColor = temperatureImage.GetComponent<Image>().color;

        controller = GameObject.Find("GameController");
        if(controller == null)
        {
            Debug.Log("Could not find GameController");
        }
      
        racers = controller.GetComponent<VehicleSpawner>().racers;
        Debug.Assert(racers.Count() != 0, "no racers wtf?");
        position.text = racers.Count().ToString();

        if (player1 != null)
        {
            player1 = racers.Last(); //FindElement(racers); //find for now, even we know the player is the last one
            playerHashCode = player1.GetHashCode();
            playerController = player1.GetComponent<PlayerController>();
        }
        else //(mainly for debugging)
        {
            playerController = GameObject.FindObjectOfType<PlayerController>(); //The PlayerController is somewhere out there
            Debug.Log("Could not find Transform player1. Trying again with FindObjectOfType from anywhere");
        }


        temperatureImage.color = Armor.fineColor;

    }

    void FixedUpdate ()
    {
		if (playerController != null && playerController.movement.Drivetrain != null) 
		{
	        time += Time.deltaTime;
	        if (time > hudUpdatingFrequency)
	        {
	            StartCoroutine(UpdateRacePosition());
	            time = 0;
	        }
	      
			UpdateVehicleCounter();
			UpdateCurrentGear();
			UpdateSpeedometer();

            if (playerController.raceStatus.gameOver)
            {
                DisplayGameOver();
            }
		}
    }

    IEnumerator UpdateRacePosition()
    {
        if (player1 != null)
        {
            position.text = (racers.OrderByDescending(x => x.position.z).ToList().FindIndex(x => x.GetHashCode() == playerHashCode) + 1).ToString(); //TODO: currently not very fast or elegant
        }
        yield return null;
    }


    private void UpdateVehicleCounter()
    {
         vehicleCount.text =  VehicleSpawner.totalNumberOfVehicles.ToString() + "  Emulated:" + VehicleSpawner.numberOfVehiclesEmulated.ToString();
    }

    private void UpdateSpeedometer()
    {
         speed.text = ((int)(playerController.movement.Drivetrain.speedRigidMph)).ToString();
    }

    private void UpdateCurrentGear()
    {
        if (playerController.movement.Drivetrain.CurrentGear == 0)
        {
            currentGear.text = "R";
        }
        else if(playerController.movement.Drivetrain.CurrentGear == 1)
        {
            currentGear.text = "N";
        }
        else
        {
            currentGear.text = (playerController.movement.Drivetrain.CurrentGear - 1).ToString();
        }
        
    }


    public void UpdateTemperature()
    {
        temperatureImage.GetComponent<Image>().color = playerController.Armor.GetColor();
    }

    #region Messaging

    public void DisplayGameOver()
    {
        if (gameOverText.gameObject.activeSelf == false)
        {
            gameOverText.gameObject.SetActive(true);
        }
    }

    public void DisplayNotification(string text)
    {
        showingNotification = true;
        upperNotfications.text = text;

        StartCoroutine(WaitUntilHide(notificationDisplayTime));
    }


    IEnumerator WaitUntilHide(float seconds)
    {
       yield return new WaitForSeconds(seconds);
        showingNotification = false;
        upperNotfications.text = "";
    }

    #endregion

}



