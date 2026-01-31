using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class playerController : MonoBehaviour
{
    public GameObject cameraObject;
    Rigidbody rb;
    public Animator handAnim;

    public float walkSpeed = 5;
    public float runSpeed = 5;

    public bool onGround = false;

    public GGJ.MaskState CurrentMaskState = GGJ.MaskState.Off;

    [SerializeField]
    public UnityEvent<GGJ.MaskState> EventMaskStateChanged;

    private bool bIsInteracting = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SetMaskState(GGJ.MaskState.Off);
    }

    public void SetMaskState(GGJ.MaskState state)
    {
        if (CurrentMaskState == state)
        {
            return;
        }

        CurrentMaskState = state;
        EventMaskStateChanged.Invoke(state);
    }

    public void OnInteractChanged(bool _bIsInteracting)
    {
        bIsInteracting = _bIsInteracting;
    }

    // Update is called once per frame
    void Update()
    {
        float speedMultiplier = 5;
        AnimatorStateInfo handInfo = handAnim.GetCurrentAnimatorStateInfo(0);

        if (Input.GetMouseButton(1))
        {
            speedMultiplier = 10;
            if (!handInfo.IsName("HoldMask") && !handInfo.IsName("EnterMask"))
            {
                handAnim.Play("EnterMask");
                SetMaskState(GGJ.MaskState.On);
            }
        }
        else
        {
            if (handInfo.IsName("HoldMask") || handInfo.IsName("EnterMask") || handInfo.IsName("RemoveMask"))
            {
                handAnim.Play("RemoveMask");
                SetMaskState(GGJ.MaskState.Off);
            }
            else
            {
                handAnim.Play("Idle");
            }
        }
        
        Vector3 vel = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * speedMultiplier;

        if (!bIsInteracting)
        {
            vel.y = rb.velocity.y;
            rb.velocity = vel;
            transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
            cameraObject.transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);
        }

        onGround = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        if (onGround)
        {
            if (Input.GetKey(KeyCode.Space) && Input.GetMouseButton(1))
            {
                vel.y = 5;
                rb.velocity = vel;
            }
        }
    }
}
