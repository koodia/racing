

using UnityEngine;

public interface IControlNobs
{
    void CheckArmorStatus();
    void TeleportToHighWay();
    void RecoveryReset();
}

public class ControlNobs : IControlNobs
{
    private BaseCar BaseCar;
    public ControlNobs(BaseCar baseCar)
    {
        this.BaseCar = baseCar;
    }

    public void CheckArmorStatus()
    {
        if (BaseCar.Armor.CurrentValue < 1)
        {
            //if (raceStatus.gameOver == false)
            //{
            //    raceStatus.gameOver = true;
            //    status.isEngineOn = false;
            //    GameManager.Instance.DisplayGameOver();
            //}
        }
    }

    public void RecoveryReset()
    {
        if (Input.GetButton("Reset"))
        {
            //TODO:
            //Find nearest Gas Station
            //Spawn
            BaseCar.transform.transform.rotation = new Quaternion(0, 0, 0, 0);
            BaseCar.rigid.velocity = Vector3.zero;
            BaseCar.rigid.angularVelocity = Vector3.zero;
            BaseCar.gameObject.transform.localPosition = new Vector3(200, 80, 8400f); ;

            BaseCar.KillTorgueFromWheels();
        }
    }

    /// <summary>
    /// Mainly for testing, later for respawning?
    /// </summary>
    public void TeleportToHighWay()
    {
        if (Input.GetKey(KeyCode.T))
        {
            BaseCar.transform.rotation = new Quaternion(0, 0, 0, 0);
            BaseCar.rigid.velocity = Vector3.zero;
            BaseCar.rigid.angularVelocity = Vector3.zero;
            BaseCar.gameObject.transform.position = new Vector3(74, 1.2f, 600f);

            BaseCar.KillTorgueFromWheels();
        }
    }
}
