
using System;
using UnityEngine;

[System.Serializable]
public class Armor
{
    public static float hitThreshold = 25000; // && < 5 scratch
    private float hitTresholdInstaDeath = Armor.hitThreshold + 170000;
    private float hitTresholdHard = Armor.hitThreshold + 120000;
    private float hitTresholdMedium = Armor.hitThreshold + 45000;
    private float hitTresholdMinor = Armor.hitThreshold + 25000;
    //private float hitTresholdScratch. == Any hit over hitThreshold

    private static Color deadColor = new Color32(96, 96, 96,255);
    private static Color jokeColor = new Color32(255, 0, 127, 255);
    private static Color criticalColor = new Color32(204, 0, 0, 255);
    private static Color weakColor = new Color32(255, 128, 0, 255);
    private static Color someHitsColor = new Color32(255, 255, 0, 255);
    private static Color scratchColor = new Color32(255, 204, 204, 255);
    public static Color fineColor = new Color32(255, 255, 255, 255);

    public static Color32[] temperatureColors = new Color32[] {
         deadColor
        ,jokeColor
        ,criticalColor
        ,weakColor
        ,someHitsColor
        ,scratchColor
        ,fineColor
    };

    public readonly float valueMax;
    private Color GaugeColor { get; set; }

    private float tempCalc = 1f;
    private float currentValue = 1; //default is 1 so gameover wont rise
    public float CurrentValue
    {
        get { return currentValue; }
        set
        {
            if (value > valueMax)
            {
                currentValue = valueMax;
            }
            else
            {
                currentValue = value;
            }
        }
    }

    ////Strong side collision means medium hit
    // NotSet = 0
    //, Fragile = 3    //Formula: 3 medium hits
    //, Weak = 10      //Sport car: 10 medium hits
    //, Medium = 20    //Sedan & Muscle: 20 medium hits
    //, Strong = 40    //Pickup : 30 medium hits
    //, HeavyDuty = 100  //Trucks
    public Armor(ArmorLevel level)
    {
        switch (level)
        {
            case ArmorLevel.NotSet:
                throw new NotImplementedException("Armorlevel cannot be 0");
            case ArmorLevel.Fragile:
                valueMax = 3;
                break;
            case ArmorLevel.Weak:
                valueMax = 7;
                break;
            case ArmorLevel.Medium:
                valueMax = 10; //for testing
                break;
            case ArmorLevel.Strong:
                valueMax = 40;
                break;
            case ArmorLevel.HeavyDuty:
                valueMax = 70;
                break;
            default: throw new NotImplementedException("");
        }

        CurrentValue = valueMax;
    }

    public void TakeHitOrDamage(bool isThePlayer, float collisionMagnitude)
    {
        if (collisionMagnitude > hitTresholdInstaDeath)
        {
            this.currentValue = 0; //GABOOOM!
        }
        else if (collisionMagnitude > hitTresholdHard)
        {
            Debug.Log("Hard hit");
            this.currentValue -= 8;
        }
        else if (collisionMagnitude > hitTresholdMedium)
        {
            Debug.Log("Medium hit");
            this.currentValue -= 3;
        }
        else if (collisionMagnitude > hitTresholdMinor)
        {
            Debug.Log("Minor hit");
            this.currentValue -= 1;
        }
        else
        {
            Debug.Log("a scrath!");
            //play effect
        }

        if (isThePlayer)
        {
            HUD.instance.UpdateTemperature();
        }
    }

    public Color32 GetColor()
    {
        tempCalc = currentValue / valueMax * 100;

        if (tempCalc >= 80)
        {
            return temperatureColors[6];
        }
        else if (tempCalc <= 80 & tempCalc > 60)
        {
            return temperatureColors[5];
        }
        else if (tempCalc <= 60 & tempCalc > 40)
        {
            return temperatureColors[4];
        }
        else if (tempCalc <= 40 & tempCalc > 30)
        {
            return temperatureColors[3];
        }
        else if (tempCalc <= 30 & tempCalc > 2 )//Wider range. More intense feel to be on the edge longer
        {
            return temperatureColors[2];
        }
        else if (tempCalc > 0)
        {
            return temperatureColors[1];
        }
        else
        {
            return temperatureColors[0];
        }
    }
}