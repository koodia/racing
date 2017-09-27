

/// <summary>
/// Starts to manipulate the game when racer distances get too great from each other
/// </summary>
public class GameplayBalancer
{
    /* 
     * All the racing and action should be happening in certain distance cap. 
     * Current estimate is around 500 meters. If the cap gets bigger the GameplayBalancer will start to do its magic:
     * - Acceleration boost and faster speeds for the players that are in last positions
     * - There might be more traffic blocking the first position driver
     * - Note!
     */
	public int RubberEffectMultiplayer{ get; set;}
	public int HappeningCap { get; set;} //meters
	public int DistanceFromFirstPlace_speedBoostStops { get; set;} //meters
	public int DistanceFromFirstPlace_accelerationBoostStops { get; set;} //meters

    //Less motortorgue so player enjoys more. But get dont keep these static
    public static  float aiMotorTorqueLimiter = 0.95f; //So: specs.torgue* 0.95f;
    public static float aiSpeedLimiter = 0.97f; //So: specs.torgue* 0.95f;

    public GameplayBalancer(int rubberEffectMultiplayer, int happeningCap, int distanceFromFirstPlace_speedBoostStops, int distanceFromFirstPlace_accelerationBoostStops)
	{
		this.RubberEffectMultiplayer = rubberEffectMultiplayer; 
		this.HappeningCap = happeningCap;
		this.DistanceFromFirstPlace_speedBoostStops = distanceFromFirstPlace_speedBoostStops;
		this.DistanceFromFirstPlace_accelerationBoostStops = distanceFromFirstPlace_accelerationBoostStops;

	}

}
