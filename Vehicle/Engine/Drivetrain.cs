
using System.Collections;
using UnityEngine;

[System.Serializable]
public class Drivetrain
{
    // powerband RPM range. Depends on car
    [ReadOnly] public float maxRPM = 8000;
    [ReadOnly] public float minEngineRPM = 1000;

    //public float optimalRPMChange = 6400;
    [ReadOnly] public float maxTorque = 0;
    public float throttle = 0; //can be a minus 1 as well
    public float nitro = 1;

    public float speedRigidKmh;
    public float speedRigidMph;
    public float currentWheelRPM = 0.0f;
    public float engineRPM = 0.0f;
    public int hp = 0;

    [SerializeField]
    public int currentGear;
    public int CurrentGear
    {
        get { return currentGear; }
        set
        {
            if (value > gearRatios.Length)
            {
                currentGear = gearRatios.Length;
            }
            else
            {
                if (currentGear < 0)
                {
                    currentGear = 0;
                }
                else
                {
                    currentGear = value;
                }
            }
        }
    }

    //public AnimationCurve EngineTorqueCurve = new AnimationCurve();
    private float transmissionEfficiency = 0.7f; //As much as 30% of the energy could be lost in the form of heat.
    private float[] gearRatios = new float[7];  //neutral + 6 gears
    private float[] gearThrottleLimiting;//neutral + 6 gears  //  gearThrottleLimiting ? engineTorqueCurve
    //private float[] autoRPMChangeUp = new float[] {100f, 7000.0f, 7000.0f, 7000.0f, 7000.0f, 8000.0f }; //smarter values
    private float[] autoRPMChangeUp;//TODO: Fix your ratio and 
    private float diffRatio = 3.42f; //between gears
    //private float tempCurrentGearEngineTorque;
    private float engineTorque = 0;
    private float torque = 0;
    private int numberOfGears;

    AudioSource engineAudio;

    //automatic settings:
    private bool automaticHack = false;
    int appropriateGear = 0;
    float gearChange = 0; //manual transmission won't have this lag. "Rewarding good playing"
    bool changingGear = false;
    public bool isReversing = false;

    private bool kikkare = false;
    private float kakkareRPM;
    // Engine orientation (typically either Vector3.forward or Vector3.right). 
    // This determines how the car body moves as the engine revs up.	
    //public Vector3 engineOrientation = Vector3.forward;

    // Apply torque to car body
    //rigidbody.AddTorque(-engineOrientation* engineTorque);

    /// <param name="maxTorque"></param>
    /// <param name="maxRPM"></param>
    /// <param name="automaticTransmission"></param>
    public Drivetrain(float maxTorque, float maxRPM, bool automaticTransmission, int gearNum)
    {
        this.maxTorque = maxTorque;
        this.maxRPM = maxRPM;
        this.currentGear = 1; //start from neutral
        //this.automaticHack = automaticTransmission;
        this.numberOfGears = gearNum;
        this.kikkare = false; //automaticTransmission;



        ////Corvette C5 hardtop reference: gearRatios = new float[] { 0, 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f }; //neutral(should be 2.90) + 6 gears 
        gearRatios = new float[] { -0.5f, 0, 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f, 0.5f }; //reverse, neutral, and 7 gears (7 for formula)
        gearThrottleLimiting = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        // gearThrottleLimiting = new float[] { 0.2f, 0.5f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f }; //neutral + 6 gears  //  gearThrottleLimiting ? engineTorqueCurve.  The first is overpowered to get some speed
        autoRPMChangeUp = new float[] { maxRPM * 1f, maxRPM * 0.5f, maxRPM * 0.9f, maxRPM * 0.9f, maxRPM * 0.9f, maxRPM * 0.9f, maxRPM * 0.9f, maxRPM * 0.9f, maxRPM * 0.9f }; //TODO: Fix your ratio and 
    }

    public void UpdateThrottleInput()
    {
        throttle = Input.GetAxis("Vertical");
    }

    public IEnumerator ActivateNitro(float ammountNitro)
    {
        this.nitro = 2;
        yield return new WaitForSeconds(ammountNitro);
        this.nitro = 1; //back to normal
    }

    /// <summary>
    /// float 1 is normal level. Float 2 is nitro on
    /// </summary>
    /// <param name="ammountNitro"></param>
    public void ActivateBoost(float ammountNitro)
    {
  
        if (ammountNitro < 1)
        {
            nitro = 1;
        }
         this.nitro = ammountNitro;
    }

        public void CalculateKmh(Rigidbody rigid)
    {
        speedRigidKmh = rigid.velocity.magnitude * 3.6f;
        speedRigidMph = speedRigidKmh / 1.609344f;
    }

    //Mieti vielä kahdesti otatko eturenkaista
    public void CalculateRPM(WheelCollider[] poweringWheels, Vector3 relativeVelocity)
    {
        //        float wheelRPM = 0;
        //        float wheelRadius = 0;
        //        bool slipping = false;
        //        float wheelSpeed = 0;

        if (CurrentGear != 1) // not neutral gear
        {
            //If we check the tyre slip
            //foreach (WheelCollider w in poweringWheels)
            //{
            //    wheelRPM = w.rpm;
            //    rpm = wheelRPM * gearRatios[currentGear] * diffRatio;
            //    //wheelRadius = w.radius;
            //    //break;
            //}

            currentWheelRPM = (poweringWheels[0].rpm + poweringWheels[1].rpm) / 2 * gearRatios[CurrentGear] * nitro * diffRatio;

            //Check slipping?
            //wheelSpeed = (wheelRPM * 2 * Mathf.PI * wheelRadius) / 60.0f;
            //if (Mathf.Abs(wheelSpeed - relativeVelocity.z) > 1.0f)
            //    slipping = true;
            //else
            //    slipping = false;

            engineRPM = Mathf.Lerp(engineRPM, (currentWheelRPM + UnityEngine.Random.Range(-50f, 50f)), Time.deltaTime * 2f);
            if (engineRPM > maxRPM)
            {
                engineRPM = maxRPM;
            }
            //For debugging:
        }
        else //calculate rpm for neutral
        {

            float temp;
            if (throttle > 0)
            {
                 temp = maxRPM * Time.deltaTime * throttle; //emulating first gear
            }
            else
            {
                 temp = maxRPM * Time.deltaTime * -throttle;
            }

            if (temp > maxRPM)
            {
                currentWheelRPM = maxRPM;
            }

            if (temp < 0)
            {
                currentWheelRPM = 0;
            }

            //currentWheelRPM = gearRatios[CurrentGear] * diffRatio * throttle; //currentGear is still 0
        }
       
        //UpdateEngineSound();
    }


    public void ApplyMotorTorque(WheelCollider[] poweringWheels, int maxSpeed, float rigidBodySpeedKmH)
    {
        //tempCurrentGearEngineTorque = EngineTorqueCurve.Evaluate(tRpm);
        //float skidControl = gearThrottleLimiting[currentGear];

        torque = 0;
        if (changingGear) //maybe have a moment wihtout torgue. Think this later
        {
            torque = 0;
            engineTorque = 0;
        }
        else
        {
            if (throttle > 0 & CurrentGear != 0 & CurrentGear != 1) //Not reverse or neutral
            {
                torque = maxTorque * throttle * gearThrottleLimiting[CurrentGear];
                hp = (int)((torque * engineRPM) / 5252); //TODO: remove when you are happy
                if (rigidBodySpeedKmH <= maxSpeed)
                {
                    engineTorque = torque * gearRatios[CurrentGear] * diffRatio * transmissionEfficiency;
                }
                else
                {
                    engineTorque = maxTorque;
                }

                SetDecelerationSpeed(engineTorque, 0, poweringWheels);

            }
            else if (throttle < 0 && CurrentGear == 0)//Reverse
            {
                SetDecelerationSpeed(engineTorque, 0, poweringWheels);
                torque = maxTorque * throttle * gearRatios[0];
                engineTorque = -(torque * diffRatio * transmissionEfficiency);

            }
            else //Neutral
            {
                //currentWheelRPM = 0;
                engineTorque = 0;
                engineRPM = 0;
                SetDecelerationSpeed(engineTorque, 3000, poweringWheels);
            }

            foreach (WheelCollider w in poweringWheels)
            {
                w.motorTorque = engineTorque;
            }
        }
    }

    //private void CheckStalling()
    //{
    //    if (engineRPM < (minEngineRPM && starting)
    //    {
    //        engineON = false;
    //    }
    //}


    private void SetDecelerationSpeed(float engineTorque, float decelerationSpeedInNeutral, WheelCollider[] poweringWheels)
    {

        // drag doesn't interfere with the physics processing but probably not the best way to slow down
        //rigdig.drag = rigdig.velocity.magnitude / 250;

        //So add use brakes
        foreach (WheelCollider w in poweringWheels)
        {
            w.motorTorque = engineTorque;
            w.brakeTorque = decelerationSpeedInNeutral;
        }
    }

    public void ShiftUp()
    {
        if (CurrentGear < numberOfGears)
        {
            CurrentGear++;
            if (CurrentGear < gearRatios.Length) //Check if there are more gears to shift up
            {
                engineRPM = 0; //drop rpm when you change gear. TODO: calculate better like:   //engineRPM = Mathf.Max(maxRPM, (float)maxRPM * (float)speed / (float)maxSpeedsPerGear[gear - 1]);
                currentWheelRPM = 0;
                //engineTorque = 0; //DONT DO IT
                torque = 0;
            }
        }
    }
    /// <summary>
    /// Supresses speed  A a bit hack a bit moment
    /// </summary>
    public void OnBraking()
    {
        engineRPM = 0;
        //rpm = 0;
        engineTorque = 0;
        torque = 0;

    }

    public void ShiftDown()
    {
        if (CurrentGear > 0)
            CurrentGear--;
    }

    public void GearShifting(WheelCollider[] poweringWheels)
    {
        if (!kikkare)
        {
            ManualShifting();
        }
        else
        {
            AutomaticShifting(poweringWheels);
        }
    }

    public void ManualShifting()
    {
        if (Input.GetButtonDown("GearUp"))
        {
            ShiftUp();
        }
        else if (Input.GetButtonDown("GearDown"))
        {
            ShiftDown();
        }
        else
        {
        }
    }

    public void AutomaticShifting(WheelCollider[] poweringWheels)
    {
        //Gear Change Time interval
        gearChange -= Time.deltaTime;

        if (isReversing)
        {
            CurrentGear = 0;
            gearChange = 1f;
        }
        //else if (GetCurrentSpeed(poweringWheels) > -0.3 && GetCurrentSpeed(poweringWheels) < 0.3 && throttle == 0)
        //{
        //    CurrentGear = 2;
        //    return;
        //}
        else
        {
            CurrentGear = 1; // neutral;
        }

        if (gearChange > 0)
            return;

        if (CurrentGear < autoRPMChangeUp.Length && engineRPM > autoRPMChangeUp[CurrentGear] && throttle > 0)
        {
            //CurrentGear++;
            ShiftUp();
            gearChange = 1f;
        }
        else
        {
            if (CurrentGear > 0 && ((engineRPM < 2500 && throttle <= 0)))
            {
                ShiftDown();
                gearChange = 1f;
            }
        }
    }
}