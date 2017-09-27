using UnityEngine;

public class Racer
{
    public Racer(string name, string registerPlate, bool isAIDriven)
    {
        this.RacerName = name;
        this.RegisterPlate = registerPlate;
       // Equipment sc = gameObject.AddComponent<Equipment>() as Equipment;
    }

    string RacerName {get; set;}
	public Points Points {get; set;}
    public Money Money {get; set;}
    CollisionTracker collisionTracker = new CollisionTracker();
    string RegisterPlate {get; set;} //Goes to car visuals
	bool IsAIDriven {get; set;}
 
    
    Upgrades Upgrades { get; set; }
}

