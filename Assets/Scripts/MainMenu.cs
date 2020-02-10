using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void PlayGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        GameHandler.handler.GameStart();
    }

    public void QuitGame() {
        Application.Quit();
    }
    public void Menu() {
        if (GameHandler.handler != null)
            Destroy(GameHandler.handler.gameObject);
        SceneManager.LoadScene(0);
    }
}
