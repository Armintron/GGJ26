using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerText : MonoBehaviour
{
    public GameObject triggerEnable;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<playerController>())
            triggerEnable.SetActive(true);
    }
}
