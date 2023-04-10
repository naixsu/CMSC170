using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cropCountScript : MonoBehaviour
{
    public static int cropValue = 0;
    Text crop;
    // Start is called before the first frame update
    void Start()
    {
        crop = GetComponent<Text> ();
    }

    // Update is called once per frame
    void Update()
    {
        crop.text = "Crop Count: " + cropValue;
    }
}
