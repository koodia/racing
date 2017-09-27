using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextEffectFadeInAndOut : MonoBehaviour {


    public Text text;

    public float startTime;
    public float fadingTime;
    public float repeatFrequency;

    // Use this for initialization
    void Start ()
    {
        Debug.Assert(fadingTime < repeatFrequency, "fadingTime needs smaller than the repeatFrequency. Text: " + text.name);
        text.color = new Color (text.color.r, text.color.g,text.color.b, 0);
       // InvokeRepeating("LoopEffect", startTime, repeatFrequency);
    }

    private void LoopEffect()
    {
        StartCoroutine(RepeatFade());
     }

    IEnumerator RepeatFade()
    {
        yield return FadeTextToFullAlpha(fadingTime/2, text);
        yield return FadeTextToZeroAlpha(fadingTime/2, text);
    }

    public IEnumerator FadeTextToFullAlpha(float t, Text i)
    {
        
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }

    }

    public IEnumerator FadeTextToZeroAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnEnable()
    {
        InvokeRepeating("LoopEffect", startTime, repeatFrequency);
    }
}


