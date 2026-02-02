using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class deathCollider : MonoBehaviour
{
    public string sceneLoad;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<playerController>())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Destroy(this);
        }
    }
}
