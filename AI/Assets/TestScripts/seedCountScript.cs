using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class seedCount : MonoBehaviour
{
    public static int seedCountVal = 0;
    Text seedScore;
    // Start is called before the first frame update
    void Start()
    {
        seedScore = GetComponent<Text> ();
    }

    // Update is called once per frame
    void Update()
    {
        seedScore.text = "Seed Count: " + seedCountVal;
    }
}
