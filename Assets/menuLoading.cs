using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class menuLoading : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void startGame()
    {
        SceneManager.LoadScene("Level0Scene");
    }

    public void stopGame()
    {
        Application.Quit();
    }

    public void menu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
