using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class menuLoading : MonoBehaviour
{
    public void startGame()
    {
        SceneManager.LoadScene("Level0Scene");
    }

    public void stopGame()
    {
        Application.Quit();
    }
}
