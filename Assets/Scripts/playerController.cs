using System.Collections;
using System.Collections.Generic;
using GGJ;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class playerController : MonoBehaviour
{

    public GameObject healthBar;
    public bool wearingMask;
    public GameObject oxygenBar;
    float health = 100;
    float oxygen = 100;

    public GameObject cameraObject;
    Rigidbody rb;
    public Animator handAnim;

    public float walkSpeed = 5;
    public float runSpeed = 5;

    public bool onGround = false;

    public bool spinningCrank = false;

    public bool insideFog = false;

    public MaskState CurrentMaskState = MaskState.Off;

    [SerializeField]
    public UnityEvent<MaskState> EventMaskStateChanged;

    private bool bIsInteracting = false;

    public Animator playerCrankAnimator;
    public GameObject playerObject;
    public GameObject playerCrank;

    [Header("Audio")]
    public AudioClip footstepSound;
    private AudioSource footstepSource;
    public AudioClip maskSound;
    private AudioSource maskSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SetMaskState(MaskState.Off);

        // Initialize footstep audio source
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepSound;
        footstepSource.loop = true;
        footstepSource.playOnAwake = false;

        // Initialize mask audio source
        maskSource = gameObject.AddComponent<AudioSource>();
        maskSource.clip = maskSound;
        maskSource.loop = false;
        maskSource.playOnAwake = false;
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


        float speedMultiplier = 3;
        AnimatorStateInfo handInfo = handAnim.GetCurrentAnimatorStateInfo(0);

        if (Input.GetMouseButton(1) && oxygen > 0 && spinningCrank == false)
        {
            if (insideFog)
                oxygen -= Time.deltaTime * 4;
            speedMultiplier = 4;
            if (!handInfo.IsName("HoldMask") && !handInfo.IsName("EnterMask"))
            {
                handAnim.Play("EnterMask");
                wearingMask = true;
                if (maskSound != null)
                {
                    maskSource.PlayOneShot(maskSound);
                }
                SetMaskState(MaskState.On);
            }
        }
        else
        {
            wearingMask = false;
            if (insideFog)
                health -= Time.deltaTime * 3;
            if (spinningCrank == false)
            {
                if (handInfo.IsName("HoldMask") || handInfo.IsName("EnterMask") || handInfo.IsName("RemoveMask"))
                {
                    handAnim.Play("RemoveMask");
                    SetMaskState(MaskState.Off);
                }
                else if (!handInfo.IsName("FirePistol"))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        handAnim.Play("FirePistol");
                        RaycastHit hit;
                        if (Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward, Camera.main.transform.forward, out hit))
                        {
                            Debug.Log("CALLED RAYCAST AND HIT " + hit.transform.gameObject.name);
                            if (hit.transform.gameObject.GetComponent<BaseEnemyScript>())
                            {
                                Debug.Log("HIT ENEMY");
                                hit.transform.gameObject.GetComponent<BaseEnemyScript>().Damage(30);
                            }
                        }
                    }
                    else
                    {
                        handAnim.Play("Idle");
                    }
                }
            }
        }
        if (health < 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        Vector3 vel = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * speedMultiplier;
        if (spinningCrank == false)
        {
            if (!bIsInteracting)
            {
                vel.y = rb.velocity.y;
                rb.velocity = vel;
                transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
                cameraObject.transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);
            }
        }
        else
        {
            GetComponent<Rigidbody>().velocity = new Vector3();
        }


        onGround = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        
        // Footstep audio logic
        if (onGround && !spinningCrank && !bIsInteracting && (Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f))
        {
            if (!footstepSource.isPlaying && footstepSound != null)
            {
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Pause();
            }
        }

        if (onGround)
        {
            if (Input.GetKey(KeyCode.Space) && Input.GetMouseButton(1))
            {
                vel.y = 5;
                rb.velocity = vel;
            }
        }
        {
            if (Input.GetKey(KeyCode.Space) && Input.GetMouseButton(1))
            {
                vel.y = 5;
                rb.velocity = vel;
            }
        }
    }
}
