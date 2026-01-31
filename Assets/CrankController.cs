using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class CrankController : MonoBehaviour
{
    [SerializeField]
    public Vector2 SelectionPoint;
    [SerializeField]
    public float RotationSpeed = 1;
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

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SelectionPoint = mouseInViewport;
            lastUpdateMousePos = mouseInViewport;
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            {
                float nearPlane = Camera.main.nearClipPlane;
                Vector3 SelectionPointInViewport = Camera.main.ViewportToWorldPoint(new Vector3(SelectionPoint.x, SelectionPoint.y, nearPlane));
                Vector3 MousePointInViewport = Camera.main.ViewportToWorldPoint(new Vector3(mouseInViewport.x, mouseInViewport.y, nearPlane));
                Debug.DrawLine(SelectionPointInViewport, MousePointInViewport, Color.red);
            }
            float deltaAngle = Vector2.SignedAngle(SelectionPoint - mouseInViewport, SelectionPoint - lastUpdateMousePos);
            transform.Rotate(transform.right, deltaAngle * RotationSpeed);
            lastUpdateMousePos = mouseInViewport;
        }
    }
}
