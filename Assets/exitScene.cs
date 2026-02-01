using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class exitScene : MonoBehaviour
{
    public string sceneLoad;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<playerController>())
        {
            SceneManager.LoadScene(sceneLoad);
            Destroy(this);
        }
    }
}
