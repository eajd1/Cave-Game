using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCast : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit))
            {
                FindObjectOfType<EndlessTerrain>().EditTerrain(hit.point, 2f, -0.1f);
            }
        }
        if (Input.GetButton("Fire2"))
        {

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit))
            {
                FindObjectOfType<EndlessTerrain>().EditTerrain(hit.point, 2f, 0.1f);
            }
        }
    }
}
