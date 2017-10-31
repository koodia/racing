using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class LoadingScreen : MonoBehaviour
{
    public Image image;

    // Use this for initialization
    void Start ()
    {
        string[] imageList = Helpers.GetFileNamesFromResourcesFolder("Images/Loading", new[] { ".jpg", ".png" });
        Random.InitState((int)System.DateTime.Now.Ticks);
        int img = Random.Range(0, imageList.Length);
        var someOtherSprite = Resources.Load<Sprite>(imageList[img]) as Sprite;
        image.sprite = someOtherSprite;
    }
}
