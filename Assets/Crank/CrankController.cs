using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class CrankController : MonoBehaviour
{
    // Where the player clicked,
    // starting the interaction
    [SerializeField]
    public Vector2 SelectionPoint;
    [SerializeField]
    public bool UsingCrank = true;

    private Vector2 lastUpdateMousePos;
    
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
        }
        // Player is crankin it
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            // Debug Line
            {
                // Need some epsilon to actually see it
                float nearPlane = Camera.main.nearClipPlane + .5f;
                Vector3 SelectionPointInViewport = Camera.main.ViewportToWorldPoint(new Vector3(SelectionPoint.x, SelectionPoint.y, nearPlane));
                Vector3 MousePointInViewport = Camera.main.ViewportToWorldPoint(new Vector3(mouseInViewport.x, mouseInViewport.y, nearPlane));
                Debug.DrawLine(SelectionPointInViewport, MousePointInViewport, Color.red);
            }
            
            float deltaAngle = Vector2.SignedAngle(SelectionPoint - mouseInViewport, SelectionPoint - lastUpdateMousePos);
            transform.Rotate(transform.right, deltaAngle);
            lastUpdateMousePos = mouseInViewport;
        }
    }
}
