
using System;
using System.Collections;
using UnityEngine;

public class CrashTimer
{
    public int ownId;
    public int racerId;
    public bool isOn;
    public float timer;
    public BaseCar basee;
    

    public CrashTimer(int racerId)
    {
        this.racerId = racerId;
        this.isOn = false;
      //   this.basee = basee;
    }

    //public IEnumerator UpdateTimer()
    //{
    //    timer -= Time.deltaTime;
    //    if (timer <= 0.0f)
    //    {
    //        isOn = false;
    //    }
    //    yield return null;
    //}

    public void UpdateTimer()
    {
        timer -= Time.deltaTime;
        if (timer <= 0.0f)
        {
            isOn = false;
        }
    }
}

//public interface ICollisionTracker
//{
//    void UpdateCollisionRacers();
//}

public class CollisionTracker
{
    private int ownId;
    public bool isListEmpty;
    public CrashTimer[] previouslyHitRacers;

    public CollisionTracker(int ownId)
    {
        this.ownId = ownId;
        // isListEmpty = true;
        isListEmpty = false;
        previouslyHitRacers = new CrashTimer[GameManager.Instance.vehicleSpawner.racers.Length];
        for (int i = 0; i < GameManager.Instance.vehicleSpawner.racers.Length; i++)
        {
            previouslyHitRacers[i] = new CrashTimer(i);
        }
    }

    public void StartCrashTimer(int racerId)
    {
        Debug.Log("CrashTimer started!");
        isListEmpty = true;
        previouslyHitRacers[racerId].isOn = true;
    }

    //TODO: rewrite later
    BaseCar temp;
    public void UpdateCollisionRacers()
    {
        if (!isListEmpty)//No need to loop empty list
        {
            for (int i = 0; i < previouslyHitRacers.Length - 1; i++)
            {
                if (i == ownId) //skip itself
                { continue; }

                isListEmpty = true;
                if (previouslyHitRacers[i].isOn)
                {
                    temp = GameManager.Instance.vehicleSpawner.racers[i].GetComponent<BaseCar>();
                    
                    isListEmpty = false;
                    previouslyHitRacers[i].UpdateTimer();
                    if (previouslyHitRacers[i].racerId == temp.temporaryWorkAroundRacerId && temp.raceStatus.gameOver) //CACHE!  //  if (previouslyHitRacers[i].racerId.RaceStatus.gameOver == true)
                    {
                        Debug.Log("YOU GOT A BONUS FROM DESTROYING A CAR");
                        //  AddBonus(Bonus.DestroyRacer);
                    }
                }
                else
                {
                    previouslyHitRacers[i].isOn = false;
                    previouslyHitRacers[i].timer = 5;
                }
            }
        }
    }
}