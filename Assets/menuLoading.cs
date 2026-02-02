using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class menuLoading : MonoBehaviour
{
    public string loadScene;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void startGame()
    {
        SceneManager.LoadScene("StoryScene");
    }
    public void CustomScene()
    {
        SceneManager.LoadScene(loadScene);
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
