
using UnityEngine;


public interface IPlayerMovement
{
    Drivetrain Drivetrain { get; set; }
    void LookForward();
    void LookBack();
    void BrakeOrReverse();
    void AirMovement();
    void Accelerate(float throttle);
    void Steer();
}

public class PlayerMovement : IPlayerMovement
{
    private BaseCar BaseCar;
    private Drivetrain drivetrain;
    public Drivetrain Drivetrain
    {
        get { return drivetrain; }
        set { drivetrain = value; }
    }

    public PlayerMovement(BaseCar baseCar, Drivetrain drivetrain)
    {
        this.BaseCar = baseCar;
        this.Drivetrain = drivetrain; //new Drivetrain(baseCar.motorTorqueMax, 7000, false, 5);
    }

    public void LookForward()
    {
        if (BaseCar.status.isLookingBehind == true)
        {
            Vector3 rot = Camera.main.transform.rotation.eulerAngles;//Camera.main.transform.position - this.transform.position;
            rot = new Vector3(rot.x, rot.y - 180, rot.z);
            Camera.main.transform.rotation = Quaternion.Euler(rot);
            //ChangeCameraPosition(-20);
            BaseCar.status.isLookingBehind = false;
        }
    }

    public void LookBack()
    {
        if (BaseCar.status.isLookingBehind == false)
        {
            Vector3 rot = Camera.main.transform.rotation.eulerAngles;//Camera.main.transform.position - this.transform.position;
            rot = new Vector3(rot.x, rot.y + 180, rot.z);
            Camera.main.transform.rotation = Quaternion.Euler(rot);
            //ChangeCameraPosition(20);
            BaseCar.status.isLookingBehind = true;
        }
    }

    //Google: Ackermann steering geometry
    public void Steer()
    {
        if (Input.GetButton("Left"))
        {
            BaseCar.LerpToSteeringAngle(-BaseCar.maxSteerAngle, false);
        }
        else if (Input.GetButton("Right"))
        {
            BaseCar.LerpToSteeringAngle(BaseCar.maxSteerAngle, false);
        }
        else
        {
            //lerppaa takas nollaan?
            BaseCar.LerpToSteeringAngle(0, false);
        }
    }

    public void BrakeOrReverse()
    {
        if (Input.GetButton("Handbrake"))
        {
            BaseCar.OnHandBrake();
        }
        else if (Input.GetButton("BrakeOrReverse"))
        {
            BaseCar.CurrentSpeed = BaseCar.wheelSpinningWithoutRpm * BaseCar.wheelRL.rpm;
            if (BaseCar.CurrentSpeed > 0)//Brake
            {
                Drivetrain.isReversing = false;
                Drivetrain.OnBraking();
                BaseCar.OnBrake();

            }
            else
            {
                Drivetrain.isReversing = true;
                BaseCar.OnReverse();
            }
        }
        else
        {
            Drivetrain.isReversing = false;
            BaseCar.OnReleaseBrake();
        }
    }

    //Keys: https://docs.unity3d.com/Manual/ConventionalGameInput.html
    public void AirMovement()
    {
        if (Input.GetButton("PitchForward"))
        {
            BaseCar.gameObject.transform.Rotate(Vector3.right * Time.deltaTime * BaseCar.yawSpeed);
        }

        if (Input.GetButton("PitchBackward"))
        {
            BaseCar.gameObject.transform.Rotate(Vector3.left * Time.deltaTime * BaseCar.yawSpeed);
        }

        if (Input.GetButton("YawRight"))
        {
            BaseCar.gameObject.transform.Rotate(Vector3.up * Time.deltaTime * BaseCar.yawSpeed);
        }

        if (Input.GetButton("YawLeft"))
        {
            BaseCar.gameObject.transform.Rotate(Vector3.down * Time.deltaTime * BaseCar.yawSpeed);
        }
    }

    /// <summary>
    /// Obsolete. Drivetrain replaces this
    /// </summary>
    public void Accelerate(float throttle)
    {
        // isReverse = false;
        if (throttle != 0)
        {
            BaseCar.status.isThrottle = true;
            BaseCar.CurrentSpeed = BaseCar.wheelSpinningWithoutRpm * BaseCar.wheelRL.rpm;

            if (throttle > 0)
            {

            }
            else if (BaseCar.CurrentSpeed < 0 && !BaseCar.status.isBraking)
            {
                BaseCar.status.isReverse = false;
                BaseCar.status.isBraking = true;
            }
            else
            {
                BaseCar.status.isReverse = true;
                BaseCar.status.isBraking = false;
            }
        }
        else //neutral
        {
            BaseCar.status.isThrottle = false;
            //KillTorgueFromWheels(); //Drivetrain will handle this
        }

        //Compensate for floating point imprecision.
        //If the player is not supposed to be moving, explicitly tell him so.
        //if (currentSpeed > -1f && currentSpeed < 1f)
        //{
        //    currentSpeed = 0.0f;
        //}
    }

}