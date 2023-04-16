using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class seedCountScript : MonoBehaviour
{
    public static int seedValue;
    [SerializeField] private Text seed;
    // Start is called before the first frame update
    void Start()
    {
        seedValue = 0;
    }

    // Update is called once per frame
    void Update()
    {
        seed.text = "Seed Count: " + seedValue;
    }
}
