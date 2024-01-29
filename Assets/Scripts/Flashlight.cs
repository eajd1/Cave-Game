using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    new Light light;
    int state = 1;
    float[] lightState = { 0f, 80f, 250f };

    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            state += 1;
            if (state > 2) { state = 0; }
            light.range = lightState[state];
        }
    }
}
