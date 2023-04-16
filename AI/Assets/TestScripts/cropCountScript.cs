using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cropCountScript : MonoBehaviour
{
    public static int cropValue;
    [SerializeField] private Text crop;
    // Start is called before the first frame update
    void Start()
    {
        cropValue = 0;
    }

    // Update is called once per frame
    void Update()
    {
        crop.text = "Crop Count: " + cropValue;
    }
}
