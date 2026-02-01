using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using GGJ;
using UnityEngine;
using UnityEngine.Events;

public class CrankController : MonoBehaviour
{
    // Where the player clicked,
    // starting the interaction
    [SerializeField]
    public Vector2 SelectionPoint;
    [SerializeField]
    public bool UsingCrank = false;
    [SerializeField]
    public float crankPercentage;
    [SerializeField]
    public UnityEvent<CrankState> EventCrankStateChanged;

    public CrankState CurrentCrankState = CrankState.NotStarted;
    private Vector2 lastUpdateMousePos;

    public Animator playerCrankAnimator;
    public GameObject playerObject;
    public GameObject playerCrank;

    void Update()
    {
        if (!UsingCrank)
        {
            return;
        }

        Vector2 mouseInViewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        // The player just started trying to turn the crank
        // Where they started will be our "reference point"
        // for turning
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SelectionPoint = mouseInViewport;
            lastUpdateMousePos = mouseInViewport;
            crankPercentage = 0;
            UpdateCrankState(CrankState.Cranking);
        }
        // Player is crankin it
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            playerObject.transform.position += (transform.position - playerCrank.transform.position)*Time.deltaTime*3;
            playerCrankAnimator.Play("Crank");
            playerCrankAnimator.Update(transform.localEulerAngles.x/10);
            // Debug Line
            {
                // Need some epsilon to actually see it
                float nearPlane = Camera.main.nearClipPlane + .5f;
                Vector3 SelectionPointInViewport = Camera.main.ViewportToWorldPoint(new Vector3(SelectionPoint.x, SelectionPoint.y, nearPlane));
                Vector3 MousePointInViewport = Camera.main.ViewportToWorldPoint(new Vector3(mouseInViewport.x, mouseInViewport.y, nearPlane));
                Debug.DrawLine(SelectionPointInViewport, MousePointInViewport, Color.red);
            }

            float deltaAngle = Vector2.SignedAngle(SelectionPoint - mouseInViewport, SelectionPoint - lastUpdateMousePos);
            transform.Rotate(transform.InverseTransformVector(transform.right), deltaAngle);
            crankPercentage += deltaAngle / 360;
            // Allow over-completion, but floor at 0
            crankPercentage = Mathf.Clamp(crankPercentage, 0, 1);
            if (crankPercentage == 1)
            {
                EventCrankStateChanged.Invoke(CrankState.Finished);
            }

            lastUpdateMousePos = mouseInViewport;
        }
        else
        {
            UpdateCrankState(CrankState.NotStarted);
            playerCrankAnimator.Play("Idle");
        }
    }

    public void OnInteract(bool bIsInteracting)
    {
        print("OnInteract");
        UpdateCrankState(bIsInteracting ? CrankState.Cranking : CrankState.NotStarted);
        UsingCrank = bIsInteracting;

        if (bIsInteracting)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void UpdateCrankState(CrankState state)
    {
        if (state == CurrentCrankState)
        {
            return;
        }

        CurrentCrankState = state;
        EventCrankStateChanged.Invoke(state);
    }
}
