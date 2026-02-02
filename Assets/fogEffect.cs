using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class fogEffect : MonoBehaviour
{
    playerController foundPlayer;
    public PostProcessVolume volume;
    public PostProcessVolume volumeLayer2;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<playerController>())
        {
            other.gameObject.GetComponent<playerController>().insideFog = true;
            volumeLayer2.enabled = true;
            volume.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<playerController>())
        {
            other.gameObject.GetComponent<playerController>().insideFog = false;
            volume.enabled = false;
            volumeLayer2.enabled = false;
        }
    }

    private void Update()
    {
        if (foundPlayer == null)
        {
            foundPlayer = FindObjectOfType<playerController>(); 
        }
        if (foundPlayer.wearingMask == true)
        {
            volumeLayer2.gameObject.SetActive(false);
        }
        else
            volumeLayer2.gameObject.SetActive(true);
    }
}
