
using System.Collections;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    float lerpTime = 0.3f;
    float currentLerpTime;

    Vector3 startPos;
    Vector3 endPos;
    RectTransform rt;
    float time;
    float flashInterval = 8;

    protected void Start()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.localPosition;
        endPos = new Vector3(1300, 0);
    }

    protected void Update()
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > flashInterval)
        {
            rt.localPosition = startPos;
            currentLerpTime = 0;
        }

        float time = currentLerpTime / lerpTime;
        rt.localPosition = Vector3.Lerp(startPos, endPos, time);
    }

}
