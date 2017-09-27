using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheel : MonoBehaviour
{

    public WheelCollider wheelCollider;
    private Vector3 wheelPosition = new Vector3();
    private Quaternion wheelRotation = new Quaternion();

    //Vissiin olemassa vähän elegantimpi tapa https://docs.unity3d.com/Manual/WheelColliderTutorial.html
    private void FixedUpdate()
    {
        wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
        transform.position = wheelPosition;
        transform.rotation = wheelRotation;
    }
}
