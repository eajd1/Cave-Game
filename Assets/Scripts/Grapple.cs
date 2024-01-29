using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public GameObject player;
    public new Transform camera;
    LayerMask groundLayer;
    SpringJoint joint;
    LineRenderer line;

    //Grapple parameters
    public float grappleMaxLength;
    float grappleLength;
    public float reelSpeed;
    Vector3 grapplePos;
    bool isGrappled;
    public float spring;
    public float damper;

    //Line parameters
    public Transform lineOrigin;
    public Material lineMaterial;
    public AnimationCurve lineWidth;

    public KeyCode reelIn;
    public KeyCode reelOut;

    void Start()
    {
        groundLayer = GetComponent<PlayerController>().groundLayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            if (!isGrappled)
            {
                RaycastHit hit;
                if (Physics.Raycast(camera.position, camera.TransformDirection(Vector3.forward), out hit, grappleMaxLength, groundLayer.value))
                {
                    isGrappled = true;
                    grapplePos = hit.point;

                    //Spring Joint
                    joint = player.AddComponent<SpringJoint>();
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = grapplePos;
                    joint.spring = spring;
                    joint.damper = damper;
                    grappleLength = Vector3.Distance(camera.position, grapplePos);

                    //Line
                    line = player.AddComponent<LineRenderer>();
                    line.SetPosition(0, lineOrigin.position);
                    line.SetPosition(1, grapplePos);
                    line.material = lineMaterial;
                    line.widthCurve = lineWidth;
                    line.numCapVertices = 10;
                }
            }
            else
            {
                Destroy(joint);
                Destroy(line);
                isGrappled = false;
            }
        }

        if (isGrappled && Input.GetKey(reelIn) && grappleLength > 0) { grappleLength -= reelSpeed; }
        if (isGrappled && Input.GetKey(reelOut) && grappleLength < grappleMaxLength) { grappleLength += reelSpeed; }

        if (isGrappled)
        {
            joint.maxDistance = grappleLength;
            line.SetPosition(0, lineOrigin.position);
        }
    }

    public bool IsGrappled()
    {
        return isGrappled;
    }
}
