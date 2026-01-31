using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickups : MonoBehaviour
{
    public enum PickupType { Oxygen }
    
    [Header("Pickup Settings")]
    public PickupType type;
    public float value = 20f;
    public float interactDistance = 5f;

    public bool CanInteract { get; private set; }
    private Transform playerCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (playerCamera == null)
        {
            CanInteract = false;
            return;
        }

        // Use a raycast from the camera center to see if we are looking at this object specifically
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;
        
        bool lookingAtThis = false;
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                lookingAtThis = true;
            }
        }

        CanInteract = lookingAtThis;

        if (CanInteract && Input.GetKeyDown(KeyCode.E))
        {
            ActivatePickup();
        }
    }

    private void ActivatePickup()
    {
        Debug.Log($"Activated {type} pickup!");
        
        switch (type)
        {
            case PickupType.Oxygen:
                ApplyOxygen();
                break;
        }
    }

    private void ApplyOxygen()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddOxygen(value);
        }
        Debug.Log($"+{value} Oxygen");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
