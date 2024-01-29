using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera mainCamera;
    public Camera secondCamera;
    public Light globalLight;
    public KeyCode switchCamera = KeyCode.Tab;
    public float scollSensitivity;

    float zoom = -30;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera.enabled = true;
        secondCamera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (secondCamera.enabled)
        {
            zoom += Input.GetAxis("Mouse ScrollWheel") * scollSensitivity;
            if (zoom > -30) { zoom = -30; }
            if (zoom < -150) { zoom = -150; }
            secondCamera.transform.localPosition = new Vector3(0, 0, zoom);
        }

        if (Input.GetKey(switchCamera))
        {
            mainCamera.enabled = false;
            secondCamera.enabled = true;
            globalLight.enabled = true;
        }
        else
        {
            mainCamera.enabled = true;
            secondCamera.enabled = false;
            globalLight.enabled = false;
        }
    }
}
