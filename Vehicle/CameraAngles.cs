using System;
using UnityEngine;

public class FixedCamera
{
    public CameraAngle[] cameraAngles = new CameraAngle[4];
    public CameraView cameraView;
    private int numberOfCameraAngles = Enum.GetNames(typeof(CameraView)).Length;

    public void SetCameraView(CameraView view)
    {
       Camera.main.transform.localRotation = new Quaternion(0, 0, 0, 0);
        switch (view)
        {
            case CameraView.BonnetView:
                Camera.main.transform.localPosition = cameraAngles[0].Position;
                Camera.main.transform.localRotation = new Quaternion(cameraAngles[0].RotationAngleX, 0, 0, 1);
                break;
            case CameraView.Frontview:
                Camera.main.transform.localPosition = cameraAngles[1].Position;
                Camera.main.transform.localRotation = new Quaternion(cameraAngles[1].RotationAngleX, 0, 0, 1);
                break;
            case CameraView.FirstPerson:
                Camera.main.transform.localPosition = cameraAngles[2].Position;
                Camera.main.transform.localRotation = new Quaternion(cameraAngles[2].RotationAngleX, 0, 0, 1);
                break;
            case CameraView.ThirdPerson:
                Camera.main.transform.localPosition = cameraAngles[3].Position;
                Camera.main.transform.localRotation = new Quaternion(cameraAngles[3].RotationAngleX, 0, 0, 1);
                break;
        }
    }

    public FixedCamera(CarPrefabName prefabName)
    {
        cameraAngles = GetCameraAngles(prefabName);
    }

    /// <summary>
    /// Keeps looping all enum CameraView angles
    /// </summary>
    /// <param name="view"></param>
    public void ChangeCameraView(CameraView view)
    {
        SetCameraView(view);
        if ((int)cameraView >= numberOfCameraAngles - 1)
        {
            cameraView = 0;
        }
        else
        {
            cameraView++;
        }
    }

    /// <summary>
    /// Keeps looping all enum CameraView angles
    /// </summary>
    /// <param name="view"></param>
    public void ChangeCameraView()
    {
        SetCameraView(cameraView);
        if ((int)cameraView >= numberOfCameraAngles - 1)
        {
            cameraView = 0;
        }
        else
        {
            cameraView++;
        }
    }


    /// <summary>
    /// carSpesificCameraAngles[0] = bonnetView
    /// carSpesificCameraAngles[1] = frontView (firstperson but lower)
    /// carSpesificCameraAngles[2] = firstPersonView
    /// carSpesificCameraAngles[3] = thirdPersonView
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public CameraAngle[] GetCameraAngles(CarPrefabName prefabName)
    {
        CameraAngle[] carSpesificCameraAngles = new CameraAngle[4];
        switch (prefabName)
        {
            case CarPrefabName.Camaro_Prefab:
                carSpesificCameraAngles[0] = new CameraAngle(new Vector3( 0, 0.87f, 0.5f),0);
                carSpesificCameraAngles[1] = new CameraAngle(new Vector3(0, 4.5f, 2.33f),0);
                carSpesificCameraAngles[2] = new CameraAngle(new Vector3(0, 0.8f, 2.33f), 0);
                carSpesificCameraAngles[3] = new CameraAngle(new Vector3(0, 1.6f, -4f), 0.1f); 
                break;
            case CarPrefabName.DodgeRam_Prefab:
                carSpesificCameraAngles[0] = new CameraAngle(new Vector3(0, 0.87f, 0.5f), 0);
                carSpesificCameraAngles[1] = new CameraAngle(new Vector3(0, 4.5f, 2.33f), 0);
                carSpesificCameraAngles[2] = new CameraAngle(new Vector3(0, 0.8f, 2.33f), 0);
                carSpesificCameraAngles[3] = new CameraAngle(new Vector3(0, 1.6f, -4f), 0.1f);
                break;
            case CarPrefabName.Ferrari_Prefab:
                carSpesificCameraAngles[0] = new CameraAngle(new Vector3(0, 0.094f, 0.012f), 0);
                carSpesificCameraAngles[1] = new CameraAngle(new Vector3(0, 0.0458f, 0.1896f), 0);
                carSpesificCameraAngles[2] = new CameraAngle(new Vector3(0, 0.0736f,  0.006f), 0);
                carSpesificCameraAngles[3] = new CameraAngle(new Vector3(0, 0.14f, -0.4f), 0.02f);
                break;
            default: throw new NotImplementedException("car has no cameralist yet!:" + prefabName);
        }

        return carSpesificCameraAngles;

    }
}

    public class CameraAngle
    {
        //public CameraView view // we can survive without this
        public Vector3 Position { get; set; }
        public float RotationAngleX { get; set; }

        public CameraAngle(Vector3 position, float rotationAngleX)
        {
            this.Position = position;
            this.RotationAngleX = rotationAngleX;
        }

    }
