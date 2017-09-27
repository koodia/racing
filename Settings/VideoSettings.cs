using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VideoSettings
 {
    //Tähän voi toistaiseksi listaa kaikki käytetyt efektit ja muut tehosyöpöt alueet
	public const int CameraFarPlane = 600; //You barely see the car disappearing //1500
    public const float AiRacersSensorFrequency = 0.35f; //"aiDriverReactionTime"
    public const float TrafficSensorFrequency = 1.0f;    //Traffic raycast frequency
    public const float MaxSpeed = 400;
    public const int VehicleCountMax = 300;


    //REWRITE and OPTIMIZE better when you have time:
    //RoadObject - CollisionDetection
    //Sensors - raycasting
}
