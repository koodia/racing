

/// <summary>
/// The features that needed to be added on top of AIBase in order to make a AIRacerCar
/// </summary>
public interface IRacer
{
    Racer Racer { get; set; }
    Armor Armor { get; set; } //the base car will already have this
    Equipment Equipment { get; set; }
    bool IsAIDriving {get; set;}
    float RigidBodySpeedKmH { get; set; }
}
