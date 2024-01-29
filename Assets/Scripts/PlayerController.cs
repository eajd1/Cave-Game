using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public new Transform camera;
    new Rigidbody rigidbody;

    float cameraPitch;
    float cameraRotation;
    Vector3 movement;
    public int sensitivity;
    public float movementSpeed;
    public float gravity = -9.8f;
    public float jumpHeight;
    public float wallCheckDistance;

    public Transform groundCheck;
    public float checkRadius = 0.4f;
    public LayerMask groundLayer;
    public bool isGrounded;
    bool prevGrappled;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundLayer);

        if (!isGrounded && GetComponent<Grapple>().IsGrappled())
        {
            prevGrappled = true;
        }
        else if (isGrounded && !GetComponent<Grapple>().IsGrappled())
        {
            prevGrappled = false;
        }

        //Camera rotation
        cameraRotation += Input.GetAxis("Mouse X") * sensitivity;
        if (cameraRotation > 180) { cameraRotation = -180f; }
        if (cameraRotation < -180) { cameraRotation = 180f; }
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * cameraRotation));

        //Camera Pitch
        cameraPitch -= Input.GetAxis("Mouse Y") * sensitivity;
        if (cameraPitch > 90) { cameraPitch = 90f; }
        if (cameraPitch < -90) { cameraPitch = -90f; }
        camera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        //Horizontal input
        movement = Vector3.zero;
        movement.x = Input.GetAxis("Horizontal");
        movement.z = Input.GetAxis("Vertical");

        //Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
        }

        if (Input.GetKeyUp(KeyCode.Escape) && !Cursor.visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void FixedUpdate()
    {
        if ((GetComponent<Grapple>().IsGrappled() || prevGrappled) && !isGrounded)
        {   //Grapple movement
            rigidbody.velocity += transform.TransformDirection(movementSpeed * Time.fixedDeltaTime * movement);
        }
        else
        {
            RaycastHit hit;
            if (!Physics.Raycast(transform.position + Vector3.up, transform.TransformDirection(movementSpeed * Time.fixedDeltaTime * movement), out hit, wallCheckDistance, groundLayer.value))
            {   //If there is no wall in the direction of travel
                if (Physics.Raycast(transform.position + Vector3.up + (transform.TransformDirection(movementSpeed * Time.fixedDeltaTime * movement).normalized * wallCheckDistance), Vector3.down, out hit, 2, groundLayer.value) && isGrounded)
                {   //Move along slope
                    Vector3 direction = (hit.point - transform.position).normalized;
                    rigidbody.MovePosition(rigidbody.position + movement.magnitude * movementSpeed * Time.fixedDeltaTime * direction);
                }
                else
                {   //Move horizontally
                    rigidbody.MovePosition(rigidbody.position + transform.TransformDirection(movementSpeed * Time.fixedDeltaTime * movement));
                }
            }
            else
            {
                Vector3 point = hit.point + Vector3.down + hit.normal * 0.4f;
                Vector3 direction = (point - rigidbody.position).normalized;
                //rigidbody.MovePosition(rigidbody.position + direction * movement.magnitude * Mathf.Cos(Vector3.Angle(movement, direction) * (Mathf.PI / 180)));
                rigidbody.MovePosition(rigidbody.position + direction * 0.05f);
            }
        }
    }
}
