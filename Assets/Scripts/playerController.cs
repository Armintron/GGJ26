using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    public GameObject cameraObject;
    Rigidbody rb;

    public float walkSpeed = 5;
    public float runSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float speedMultiplier = 5;
        if (Input.GetMouseButton(1))
            speedMultiplier = 10;
        Vector3 vel = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * speedMultiplier;
        vel.y = rb.velocity.y;
        rb.velocity = vel;
        transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
        cameraObject.transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);
    }
}
