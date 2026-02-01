using System.Collections;
using System.Collections.Generic;
using GGJ;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class playerController : MonoBehaviour
{
    public GameObject healthBar;
    public GameObject oxygenBar;
    float health = 100;
    float oxygen = 100;

    public GameObject cameraObject;
    Rigidbody rb;
    public Animator handAnim;

    public float walkSpeed = 5;
    public float runSpeed = 5;

    public bool onGround = false;

    public MaskState CurrentMaskState = MaskState.Off;

    [SerializeField]
    public UnityEvent<MaskState> EventMaskStateChanged;

    private bool bIsInteracting = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SetMaskState(MaskState.Off);
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetMaskState(MaskState state)
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
        healthBar.transform.localScale = new Vector3(health/100, 1, 1);
        oxygenBar.transform.localScale = new Vector3(oxygen / 100, 1, 1);

        float speedMultiplier = walkSpeed;
        AnimatorStateInfo handInfo = handAnim.GetCurrentAnimatorStateInfo(0);

        if (Input.GetMouseButton(1) && oxygen > 0)
        {
            oxygen -= Time.deltaTime * 6;
            speedMultiplier = runSpeed;
            if (!handInfo.IsName("HoldMask") && !handInfo.IsName("EnterMask"))
            {
                handAnim.Play("EnterMask");
                SetMaskState(MaskState.On);
            }
        }
        else
        {
            health -= Time.deltaTime * 3;
            if (handInfo.IsName("HoldMask") || handInfo.IsName("EnterMask") || handInfo.IsName("RemoveMask"))
            {
                handAnim.Play("RemoveMask");
                SetMaskState(MaskState.Off);
            }
            else
            {
                handAnim.Play("Idle");
            }
        }
        if (health < 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
