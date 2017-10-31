using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class PlayerVehicleCreationTests {

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
	[UnityTest]
	public IEnumerator InstantiatePlayerVehicle()
    {
        VehicleSpawner vehicleSpawner = new VehicleSpawner();
        vehicleSpawner.InstantiatePlayer(CarPrefabName.Camaro_Prefab.ToString(), Vector3.zero, false);

        yield return null;

        var result = GameObject.Find("Racer 1(Player)");
        Assert.IsTrue(result != null);
    }

    [UnityTest]
    public IEnumerator InstantiatePlayerVehicleSettings()
    {
        GameObject valid = Resources.Load("Tests/Racer 1(Player)") as GameObject;
        GameObject.Instantiate(Resources.Load("Tests/Main Camera") as GameObject);
             
        GameManager.Instance.InitializeGame(GameMode.Tournament, false, false, false, "RacerCamaro_Prefab");
        yield return null;

        var result = GameObject.Find("Racer 1(Player)");
        var result2 = GameObject.Find("Racer 0(Player)");
        Assert.AreEqual(valid.GetComponent<PlayerController>(), result.GetComponent<PlayerController>());
    }

    [TearDown]
    public void CleanScene()
    {
        var player = GameObject.Find("Racer 1(Player)");
        Object.Destroy(player);
        //var moreplayers = GameObject.FindObjectsOfType<PlayerController>();
        //foreach (var gameObject in moreplayers)
        //{
        //    Object.Destroy(gameObject);
        //}
        //var otherRacers = GameObject.FindObjectsOfType<AIRacerController>();
        //foreach (var gameObject in otherRacers)
        //{
        //    Object.Destroy(gameObject);
        //}
    }

    //jatka: https://www.youtube.com/channel/UCTjnCCcuIbrprhOiaDJxxHA
}
