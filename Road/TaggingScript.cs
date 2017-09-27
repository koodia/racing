using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//SCARY! Careful with this one! You might shoot yourself!
public class TaggingScript : MonoBehaviour {

    public string tagName = "";

	// Use this for initialization
	void Start ()
    {
        //sub children
        var more = gameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in more)
        {
            if(t.gameObject.tag == "Untagged")
            t.gameObject.tag = tagName;
        }
    }
	
}
